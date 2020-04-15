---
layout: post
title:  Functional Agent
cover: /assets/images/functional-agent/functional-agent.png
permalink: functional-agent
date: 2019-12-04 07:30:00 -0400
updated: 2019-12-04 07:30:00 -0400
categories: 
  - F#
  - F# Advent
  - MailboxProcessor
  - Asynchronous
comments: true
---

Happy F# Advent! This post demonstrates how to use
F#'s MailboxProcessor to create agents for managing
asynchronous workflows. We will create _functional_
agents in the sense that we will use functional
programming techniques such as immutable data
structures and `fold` functions to maintain state.

## Agents
An agent, or actor as it is sometimes called,
is essentially a message queue which encapsulates
instructions on how to process incoming messages. 

Agents receive messages and process them in order,
optionally maintaining some internal state.

## Use Case
In this post, we will implement a few different kinds of agents to
handle concurrency in different ways. The motivation for these agents
was a need to process a large amount of API requests
in order to load data into a warehouse. The requirements were the following:

1. Don't exceed the rate limit of the API.
2. Don't exceed the memory limits of the server processing the requests.
3. Process the requests as quickly as possible as there is a lot of data.

For an example to play with, we will implement a typewriter, where each
key press represents a request to an API, and the key info represents
the data returned from the API. Writing each line represents writing
the data to a file in order to batch upload to the warehouse. We will implement
three agents to handle our requirements.

1. Parallel Agent -> specify the work that is processed concurrently
2. Rate Agent -> specify the requests per second (key presses per second)
3. Buffer Agent -> specify the data held in memory before writing to disk (number of keys in line)

## TL;DR;
To try the typewriter, first install [Docker](https://docs.docker.com/v17.09/engine/installation/)
and [Docker Compose](https://docs.docker.com/compose/install/). Then clone the repo
and run the typewriter.
```shell
git clone https://github.com/ameier38/functional-agent.git
cd functional-agent
docker-compose run --rm typewriter
```
> This runs the typewriter application with a rate limit (`-r`)
of 2 keys per second, a parallel limit (`-p`) of 2,
a buffer size (`-b`) of 10, and writes to the `test.txt` output file.
Update the [docker-compose.yml](https://github.com/ameier38/functional-agent/blob/master/docker-compose.yaml)
file to change the parameters.

![typewriter](typewriter.gif)

> On the left you can see the rate limited typing and on the right you
can see the the keys written to a file with max buffer (read: line) of 10 keys.

## Design
We will implement our typewriter in F# with the
the built-in [MailboxProcessor](https://msdn.microsoft.com/en-us/visualfsharpdocs/conceptual/control.mailboxprocessor%5B'msg%5D-class-%5Bfsharp%5D).
We will keep track of our internal agent state by implementing an `evolve`
function for each agent in the form of `State -> Message -> State`.
For those familiar with event sourcing this should look familiar (just
replace `Message` with `Event`). For those who have worked with
front-end frameworks such as [Redux](https://redux.js.org/basics/reducers#handling-actions) 
this is commonly called the reducer. This pattern shows up in a lot of places because
it is a great way for managing and, more importantly, reasoning about state
changes. One can walk through the mental flow of _when the thing is in
this state, and something happened, what is the new state? :thinking:_ This pattern
works especially well for asynchronous processes as it allows you to
think about the flow synchronously.

### Parallel Agent
The first agent we will implement is the `ParallelAgent`. This agent will
allow us to specify exactly how many asynchronous workflows to run in parallel.
First we will define our state.

```fsharp
type ParallelAgentState =
    { IsWaiting: bool                   // Flag to track if we are done processing all work
      WorkRequestedCount: int           // How many workflows we have requested so we can make sure we process all
      WorkRunningCount: int             // How many workflows are currently running for logging
      WorkCompletedCount: int           // How many workflows have completed to compare against total requested
      WorkQueued: Queue<Async<unit>> }  // The queue of workflows to run
```

Next we will define the messages that agent can receive.

```fsharp
type ParallelAgentMessage =
    | WorkRequested of Async<unit>      // New work to be processed
    | WorkCompleted                     // Message sent after work has finished
    | WaitRequested                     // Message sent after all work has been requested
    | StatusRequested of AsyncReplyChannel<ParallelAgentStatus> // Message sent to report status of agent state
```

We can then define our implementation of the `MailboxProcessor` as

```fsharp
type ParallelAgentMailbox = MailboxProcessor<ParallelAgentMessage>
```

With our state, message, and mailbox defined, we can now work on implementing our evolve function
(remember `State -> Message -> State`).

```fsharp
let evolve (inbox:ParallelAgentMailbox) 
    : ParallelAgentState -> ParallelAgentMessage -> ParallelAgentState =
    fun (state:ParallelAgentState) (msg:ParallelAgentMessage) ->
        match msg with
        // When work is requested, increment the work requested count
        // and add the work to the queue.
        | WorkRequested work ->
            { state with
                WorkRequestedCount = state.WorkRequestedCount + 1
                WorkQueued = state.WorkQueued.Conj(work) }
        // When work is completed decrement the running work count
        // and increment the work completed count
        | WorkCompleted ->
            { state with
                WorkRunningCount = state.WorkRunningCount - 1
                WorkCompletedCount = state.WorkCompletedCount + 1 }
        // When the status is requested (for logging) we use a side effect to
        // reply to the caller using the channel that is passed with the message
        | StatusRequested replyChannel ->
            let isComplete = state.WorkRequestedCount = state.WorkCompletedCount
            if state.IsWaiting && isComplete then 
                replyChannel.Reply(Done)
            else
                let status =
                    { IsWaiting = state.IsWaiting
                      WorkRequestedCount = state.WorkRequestedCount
                      WorkRunningCount = state.WorkRunningCount
                      WorkQueuedCount = state.WorkQueued.Length
                      WorkCompletedCount = state.WorkCompletedCount }
                replyChannel.Reply(Running(status))
            state
        // When a wait is requested we have finished requesting all the work
        // and we should just update the state to indicate we are waiting for
        // all of the work to finish
        | WaitRequested -> 
            { state with
                IsWaiting = true }
        // See below
        |> tryWork inbox
```

On every message we will try to process the queued work. We will inspect the
state and see if the `WorkRunningCount` is below whatever parallel limit we set. 
If it is we will process the next work in the queue, otherwise we will do nothing.

```fsharp
let tryWork (inbox:ParallelAgentMailbox) (state:ParallelAgentState) =
    match state.WorkQueued with
    // If the queue is empty there is nothing to do
    | Queue.Nil -> state
    // If the queue is not empty, and the running count is below the limit
    // then process the next item in the queue in a new thread without 
    // waiting and update the state. When the work finishes we will send 
    // a `WorkCompleted` message.
    | Queue.Cons (work, remainingQueue) when state.WorkRunningCount < limit ->
        Async.Start(async {
            do! work
            inbox.Post(WorkCompleted)
        })
        { state with
            WorkRunningCount = state.WorkRunningCount + 1
            WorkQueued = remainingQueue }
    | _ -> state
```

Next, we will start the mailbox processor and wait for messages to be received.

```fsharp
let agent = ParallelAgentMailbox.Start(fun inbox ->
    AsyncSeq.initInfiniteAsync (fun _ -> inbox.Receive())
    |> AsyncSeq.fold (evolve inbox) initialState
    |> Async.Ignore
)
```

With the `initialState` defined as the following.

```fsharp
let initialState =
    { IsWaiting = false
      WorkRequestedCount = 0
      WorkRunningCount = 0
      WorkCompletedCount = 0
      WorkQueued = Queue.empty<Async<unit>> }
```

We use the [AsyncSeq](https://fsprojects.github.io/FSharp.Control.AsyncSeq/index.html)
library to create an infinite asynchronous sequence of received messages,
which we will then fold over using our `evolve` function and an initial state :astonished:.
This is my favorite part because it provides a really clean way to handle the incoming
messages and focuses our attention on the `evolve` function. 

Lastly, we can wrap this all up in a class with some methods to handle the messages.
```fsharp
type ParallelAgent(name: string, limit:int) =
    let initialState = ...

    let tryWork (inbox:ParallelAgentMailbox) (state:ParallelAgentState) = ...

    let evolve (inbox:ParallelAgentMailbox) = ...

    let agent = ParallelAgentMailbox.Start(fun inbox ->
        AsyncSeq.initInfiniteAsync (fun _ -> inbox.Receive())
        |> AsyncSeq.fold (evolve inbox) initialState
        |> Async.Ignore
    )

    // Function to poll until the agent is done processing all the work
    let rec wait () =
        async {
            match agent.PostAndReply(StatusRequested) with
            | Done -> ()
            | Running status ->
                Log.Information("[ParallelAgent {Name}] Status: {@Status}", name, status)
                do! Async.Sleep(1000)
                return! wait()
        }

    // Method to request new work
    member __.Post(work:Async<unit>) =
        agent.Post(WorkRequested(work))

    // Method to log the status. The argument of PostAndReply is of the form
    // AsyncReplyChannel<'Reply> -> ParallelAgentMessage
    member __.LogStatus() =
        let status = agent.PostAndReply(StatusRequested)
        Log.Information("[ParallelAgent {Name}] Status: {@Status}", name, status)

    // Send a message to indicate that all the work has been requested
    // and then poll until the agent has completed all the work
    member __.Wait() =
        agent.Post(WaitRequested)
        wait()
        |> Async.RunSynchronously
```

### Rate Agent
For this agent we will implement a [token bucket](https://en.wikipedia.org/wiki/Token_bucket)
algorithm in which we refill 'tokens' at a set interval and as long as there
are 'tokens' available then we will process the work.

Again we start off by defining the state, messages, and mailbox.

```fsharp
type RateAgentState =
    { IsWaiting: bool
      TokenCount: int
      WorkQueued: Queue<RateAgentWork> }
    
type RateAgentMessage =
    | WorkRequested of RateAgentWork
    | RefillRequested
    | WaitRequested
    | StatusRequested of AsyncReplyChannel<RateAgentStatus>

type RateAgentMailbox = MailboxProcessor<RateAgentMessage>
```

Next we will implement the `evolve` function.

```fsharp
let tryWork (state:RateAgentState) =
    let rec recurse (s:RateAgentState) =
        match s.WorkQueued, s.TokenCount with
        | Queue.Nil, _ -> s
        | _, 0 -> s
        | Queue.Cons (work, remainingQueue), tokenCount ->
            work ()
            let newState =
                { s with
                    TokenCount = tokenCount - 1
                    WorkQueued = remainingQueue }
            recurse newState
    // If there are 'tokens' available, then process the
    // queued work until the token count is zero
    if state.TokenCount > 0 then recurse state
    else state

let evolve
    : RateAgentState -> RateAgentMessage -> RateAgentState = 
    fun (state:RateAgentState) (msg:RateAgentMessage) ->
        match msg with
        // When work is requested add the work to the queue
        | WorkRequested work -> 
            { state with
                WorkQueued = state.WorkQueued.Conj(work) }
        // Increase the token count by the rate limit (tokens/second).
        // We will request a refill once per second.
        | RefillRequested ->
            { state with
                TokenCount = 1<second> * rateLimit }
        | WaitRequested -> 
            { state with
                IsWaiting = true }
        | StatusRequested replyChannel ->
            if state.IsWaiting && state.WorkQueued.IsEmpty then 
                replyChannel.Reply(Done)
            else
                let status =
                    { IsWaiting = state.IsWaiting
                      TokenCount = state.TokenCount
                      WorkQueuedCount = state.WorkQueued.Length }
                replyChannel.Reply(Running(status))
            state
        |> tryWork
```

And just like with the `ParallelAgent` we can wrap it in class to expose methods.

```fsharp
type RateAgent(name:string, rateLimit:PerSecond) =

    let initialState =
        { IsWaiting = false
          TokenCount = 1<second> * rateLimit
          WorkQueued = Queue.empty<RateAgentWork> }

    let tryWork (state:RateAgentState) = ...

    let evolve = ...

    let agent = RateAgentMailbox.Start(fun inbox ->
        AsyncSeq.initInfiniteAsync (fun _ -> inbox.Receive())
        |> AsyncSeq.fold evolve initialState
        |> Async.Ignore
    )

    let rec wait () =
        async {
            match agent.PostAndReply(StatusRequested) with
            | Done -> ()
            | Running status ->
                Log.Information("[BufferAgent {Name}] Status: {@Status}", name, status)
                do! Async.Sleep(1000)
                return! wait()
        }
    
    // Refill the tokens every second since we defined the limit as per second
    let rec refill () =
        async {
            do! Async.Sleep(1000)
            agent.Post(RefillRequested)
            return! refill()
        }

    // Start the refill in the background which will run for the life of the agent
    do Async.Start(refill())

    member __.LogStatus() =
        let status = agent.PostAndReply(StatusRequested)
        Log.Information("[RateAgent {Name}] Status: {@Status}", name, status)

    member __.Post(work:RateAgentWork) =
        agent.Post(WorkRequested(work))

    member __.Wait() =
        agent.Post(WaitRequested)
        wait()
        |> Async.RunSynchronously

```

The beauty of implementing the agents in this way is that the overall structure remains
the same and we just need to focus on the `evolve` function to update the state.

### Buffer Agent
We will leave this to the reader to explore. All the code is [available on GitHub](https://github.com/ameier38/functional-agent).

### Typewriter
We can now use our agents to implement our typewriter.

```fsharp
type Typewriter(rateLimit:int<1/second>, parallelLimit:int, bufferSize:int, filePath:string) =
    // write the buffer to a file after a delay
    let processBuffer (buffer:char list) =
        async {
            do! Async.Sleep(5000)
            let line =
                buffer 
                |> List.rev
                |> List.toArray
                |> String
            File.AppendAllLines(filePath, [line])
        }
    
    let rateAgent = RateAgent("Type", rateLimit)
    let parallelAgent = ParallelAgent("Type", parallelLimit)
    let bufferAgent = BufferAgent("Print", bufferSize, processBuffer)

    member __.Write(keyInfo:ConsoleKeyInfo) =
        match keyInfo.Key, keyInfo.Modifiers with
        // on Ctrl-Enter wait for the agents to finish
        | ConsoleKey.Enter, ConsoleModifiers.Control ->
            rateAgent.Wait()
            parallelAgent.Wait()
            bufferAgent.Wait()
            exit 0
        // on Enter log the agent status
        | ConsoleKey.Enter, _ ->
            rateAgent.LogStatus()
            parallelAgent.LogStatus()
            bufferAgent.LogStatus()
        // any other keys send the key first to the rate agent
        // which sends it to the parallel agent which sends
        // it to the buffer agent after a delay
        | _ ->
            let work = async {
                do! Async.Sleep(1000)
                bufferAgent.Post(keyInfo.KeyChar)
            }
            rateAgent.Post(fun () ->
                Console.Write(keyInfo.KeyChar)
                parallelAgent.Post(work)
            )
```

The typewriter is overly complicated but serves as a good example of
how the agent settings can affect the process. Try changing the parameters
and see how the typewriter performs!

## Summary
I hope you enjoyed the post! We covered how to implement agents in a functional
style which offers a clean way to manage state. Leave a comment if you have
any questions or if you think I could improve anything. Happy Holidays :christmas_tree:!

P.S. Thanks Sergey Tihon for organizing F# Advent!

## Resources
- [F# Weekly](https://sergeytihon.com/)
- [F# Messages and Agents](https://fsharpforfunandprofit.com/posts/concurrency-actor-model/)
- [Limit degree of parallelism using an agent](http://www.fssnip.net/nX/title/Limit-degree-of-parallelism-using-an-agent)
- [F# AsyncSeq](https://fsprojects.github.io/FSharp.Control.AsyncSeq/library/AsyncSeq.html)
- [F# Immutable Queue](https://fsprojects.github.io/FSharpx.Collections/reference/fsharpx-collections-queue-1.html)
