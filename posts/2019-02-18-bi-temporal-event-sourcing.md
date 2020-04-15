---
layout: post
title:  Bi-Temporal Event Sourcing with Equinox
cover: /assets/images/bi-temporal-event-sourcing/cover.png
permalink: bi-temporal-event-sourcing
date: 2019-03-04 12:08:00 -0400
updated: 2019-03-04 08:06:00 -0400
categories: 
  - F#
  - DDD
  - event sourcing
  - Equinox
  - bi-temporal
comments: true
---

In this post, we will demonstrate how event sourcing 
can be used to model a bi-temporal domain. We will inspect
an app written in F# that leverages Jet.com's open source library 
[Equinox](https://github.com/jet/equinox).

## What is event sourcing?
Event sourcing is a way of modeling a system such that the state of the
system is determined by replaying a series of events. It focuses on
storing the events instead of storing the state.
> See the [Resources](#resources) section for more information on event sourcing.

## What is Equinox?
__Equinox__ is A unified programming model for event-sourced command 
processing and projections for stream-based stores. In other words, 
it is a collection of .NET libraries to help build event-sourced systems.

## What is a bi-temporal domain?
A bi-temporal domain is one in which two timelines are used to determine
state. Typically these timelines are the entity time and event time. This
is illustrated in the below image. 

![bi-temporal](bi-temporal.svg)

As events occur along the event timeline, they can be effective at a 
different date in the entity timeline. The state of the entity is determined
by the ordering the events according to the entity timeline and
replaying them against an initial state. The event timeline can then be 
used to get the state of the entity at any point in time by filtering 
which events should be included in the entity timeline. A good way to determine
if a domain is bi-temporal is to ask yourself if any events in the domain
can be applied retroactively.

## Lease API
In this post, we will inspect an API that models a lease (e.g., car lease).
I chose a lease because it is an imaginary thing, an agreement between parties,
and IRL the parties involved can decide to apply some change retroactively.

### Requirements
- A user should be able to create a lease, modify a lease,
schedule a payment, make a payment, terminate a lease, and undo any action.
- A user should be able to apply any of the above actions in the past.
- A user should be able to get the state _as of_ any point in time, meaning
the state that includes all events, including retroactively applied events.
- A user should be able to get the state _as at_ any point in time, meaning
the state that only included events that occurred at and prior to that point
in time.
- A user should be able to audit all the actions that have occurred.

### Dependencies
- [Docker](https://andrewmeier.dev/win-dev#docker)

### Running the API
You should first clone the repository,
```shell
git clone https://github.com/ameier38/equinox-tutorial
```
which has the following structure:
```text
Lease
├── paket.references        --> Dependencies
├── openapi.yaml            --> Available endpoints in OpenAPI config
├── Lease.Config.fs         --> Application configuration
├── Lease.SimpleTypes.fs    --> Definitions for simple types and measures
├── Lease.Domain.fs         --> Lease commands, events, and possible states
├── Lease.Dto.fs            --> Data transfer objects
├── Lease.Aggregate.fs      --> Main business logic
├── Lease.Store.fs          --> Set up for Event Store
├── Lease.Service.fs        --> Interfaces consumed by API
├── Lease.Api.fs            --> Route handlers
└── Program.fs              --> Application entry point
```

Once you have the repo cloned, you can start the
[Event Store](https://eventstore.org/docs/event-sourcing-basics/index.html)
database and the API by running the following Docker command:
```shell
cd equinox-tutorial
docker-compose up -d
```
> The API is running at `http://localhost:8080` and the Event Store
admin site can be accessed at `http://localhost:2113` with the username
and password `admin:changeit`.

You can then start using the API. For example, to a create a lease, run:
```shell
curl -X POST \
  http://localhost:8080/lease \
  -H 'Content-Type: application/json' \
  -d '{
  "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "startDate": "2017-07-21T17:32:28Z",
  "maturityDate": "2018-07-21T17:32:28Z",
  "monthlyPaymentAmount": 25
}'
```
> All the endpoints are documented via SwaggerHub 
[here](https://app.swaggerhub.com/apis-docs/ameier38/Lease/1.0.0)

### Lease Domain
Now that we have the API running, let's explore some of the 
code to see how we have handled the requirements. First, look
at the `Lease.Domain.fs` file. This file defines all the commands,
events, and possible states of a lease. If we look at the `LeaseEvent`,
we will notice that there is a `Context` added to the payload of some
of the events.
```fsharp
type Context =
    { EventId: EventId
      CreatedDate: CreatedDate
      EffectiveDate: EffectiveDate }

type LeaseInfo =
    { Lease: Lease
      Context: Context }

type PaymentInfo =
    { Payment: Payment
      Context: Context }

type LeaseEvent =
    | Undid of EventId
    | Compacted of LeaseEvent[]
    | Created of LeaseInfo
    | Modified of LeaseInfo
    | PaymentScheduled of PaymentInfo
    | PaymentReceived of PaymentInfo
    | Terminated of Context
    interface TypeShape.UnionContract.IUnionContract
```

The `Context` record is how we track both the event timeline
(using the `CreatedDate`) and the entity timeline (using the `EffectiveDate`).

Next, let's look at the `Lease.Aggregate.fs` file. In this file you will see
a type called `StreamState`:
```fsharp
type StreamState<'DomainEvent> = 
    { NextId: EventId 
      Events: 'DomainEvent list }
```

As events occur in the system, domain events are either added to or
removed from the `StreamState.Events` list. This list of domain events
is then used to determine the state of the lease.

For example, we start with the initial stream state,
```fsharp
{ NextId = 0
    Events = [] }
```
then create a lease,
```fsharp
{ NextId = 1
    Events = [Created] }
```
then schedule a payment,
```fsharp
{ NextId = 2
    Events = [Created, PaymentScheduled] }
```
then undo the scheduled payment.
```fsharp
{ NextId = 2
    Events = [Created] }
```

In the `Lease.Aggregate.fs` file, this process is handled by the `evolve`
function:
```fsharp
let evolve : Evolve<LeaseEvent> =
    fun ({ NextId = nextId; Events = events } as state) event ->
        match event with
        | Undid undoEventId -> 
            let filteredEvents =
                events
                |> List.choose (fun e -> LeaseEvent.getEventId e |> Option.map (fun eventId -> (eventId, e)))
                |> List.filter (fun (eventId, _) -> eventId <> undoEventId)
                |> List.map snd
            { state with 
                NextId = nextId + %1
                Events = filteredEvents }
        | Compacted events ->
            { state with
                Events = List.ofArray events }
        | _ -> 
            { state with 
                NextId = nextId + %1
                Events = event :: state.Events }
```

which has the following signature:
```fsharp
type Evolve<'DomainEvent> = 
    StreamState<'DomainEvent>
     -> 'DomainEvent
     -> StreamState<'DomainEvent>
```

In order to build the state of the lease, we use the `apply` function
which has the following signature:
```fsharp
type Apply<'DomainEvent,'DomainState> =
    'DomainState
     -> 'DomainEvent
     -> 'DomainState
```

The rebuilding of the lease state is handled by the `reconstitute` function
which folds the events from `StreamState.Events` using the `apply` function
starting from an initial state of `NonExistent`. The `reconstitute` function
also takes and `ObservationDate` which is used to get the state of the lease at
any point in time by filtering the events on the `CreatedDate` or `EffectiveDate`
depending on if the query is `AsAt` or `AsOf` respectively.
```fsharp
module LeaseEvent =
    let (|Order|) { CreatedDate = createdDate; EffectiveDate = effDate } = (effDate, createdDate)
    let getContext = function
        | Undid _ -> None
        | Compacted _ -> None
        | Created { Context = ctx } -> ctx |> Some
        | Modified { Context = ctx } -> ctx |> Some
        | PaymentScheduled { Context = ctx } -> ctx |> Some
        | PaymentReceived { Context = ctx } -> ctx |> Some
        | LeaseEvent.Terminated ctx -> ctx |> Some
    let getOrder = getContext >> Option.map (fun (Order order) -> order)

let onOrBeforeObservationDate 
    observationDate 
    (effectiveDate: EffectiveDate, createdDate: CreatedDate) =
    match observationDate with
    | Latest -> true
    | AsOf asOfDate ->
        effectiveDate <= %asOfDate
    | AsAt asAtDate ->
        createdDate <= %asAtDate

let reconstitute : Reconstitute<LeaseEvent,LeaseState> =
    fun observationDate events ->
        events
        |> List.choose (fun e -> LeaseEvent.getOrder e |> Option.map (fun o -> (o, e)))
        |> List.filter (fun (o, _) -> onOrBeforeObservationDate observationDate o)
        |> List.sortBy fst
        |> List.map snd
        |> List.fold apply NonExistent
```

The `evolve` function is then wired up in the `Lease.Store.fs` file using
the Equinox library.
```fsharp
// omitted rest for brevity

let gateway = GesGateway(conn, GesBatchingPolicy(maxBatchSize=500))
let accessStrategy = Equinox.EventStore.AccessStrategy.RollingSnapshots (aggregate.isOrigin, aggregate.compact)
let cacheStrategy = Equinox.EventStore.CachingStrategy.SlidingWindow (cache, TimeSpan.FromMinutes 20.)
let serializationSettings = Newtonsoft.Json.JsonSerializerSettings()
let codec = Equinox.UnionCodec.JsonUtf8.Create<LeaseEvent>(serializationSettings)
let initial = { NextId = %0; Events = [] }
let fold = Seq.fold aggregate.evolve
GesResolver(gateway, codec, fold, initial, accessStrategy, cacheStrategy)
```

The returned resolver is then wired up in `Lease.Service.fs` to expose easy
to use `execute` and `query` functions.
```fsharp
// omitted rest for brevity

let (|AggregateId|) (leaseId: LeaseId) = Equinox.AggregateId(aggregate.entity, LeaseId.toStringN leaseId)
let (|Stream|) (AggregateId leaseId) = Equinox.Stream(log, resolver.Resolve leaseId, 3)
let execute (Stream stream) command = stream.Transact(aggregate.interpret command)
let query : Query<LeaseId,LeaseEvent,'View> =
    fun (Stream stream) (obsDate:ObservationDate) (projection:Projection<LeaseEvent,'View>) -> 
        stream.Query(projection obsDate)
        |> AsyncResult.ofAsync
```

### Example Workflow
In this section we will run through an example workflow to see the API in action.

First create a new lease.
```shell
curl -X POST \
  http://localhost:8080/lease \
  -H 'Content-Type: application/json' \
  -d '{
  "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0853",
  "startDate": "2017-07-21Z",
  "maturityDate": "2018-07-21Z",
  "monthlyPaymentAmount": 25
}'
```
```json
{
    "amountDue": 0,
    "createdDate": "2019-03-04T16:52:54.6570655Z",
    "events": [
        {
            "createdDate": "2019-03-04T16:52:54.6570655Z",
            "effectiveDate": "2017-07-21T00:00:00+00:00",
            "eventId": 0,
            "eventType": "Created"
        }
    ],
    "lease": {
        "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0853",
        "maturityDate": "2018-07-21T00:00:00+00:00",
        "monthlyPaymentAmount": 25,
        "startDate": "2017-07-21T00:00:00+00:00"
    },
    "status": "Outstanding",
    "totalPaid": 0,
    "totalScheduled": 0,
    "updatedDate": "2019-03-04T16:52:54.6570655Z"
}
```

Next, schedule and receive a payment.
```shell
curl -X POST \
  http://localhost:8080/lease/d290f1ee-6c54-4b01-90e6-d701748f0853/schedule \
  -H 'Content-Type: application/json' \
  -d '{
  "paymentDate": "2017-07-22Z",
  "paymentAmount": 25
}'
```
```shell
curl -X POST \
  http://localhost:8080/lease/d290f1ee-6c54-4b01-90e6-d701748f0853/payment \
  -H 'Content-Type: application/json' \
  -d '{
  "paymentDate": "2017-07-23Z",
  "paymentAmount": 25
}'
```
```json
{
    "amountDue": 0,
    "createdDate": "2019-03-04T16:52:54.6570655Z",
    "events": [
        {
            "createdDate": "2019-03-04T16:55:22.8958666Z",
            "effectiveDate": "2017-07-23T00:00:00+00:00",
            "eventId": 2,
            "eventType": "PaymentReceived"
        },
        {
            "createdDate": "2019-03-04T16:53:32.8943226Z",
            "effectiveDate": "2017-07-22T00:00:00+00:00",
            "eventId": 1,
            "eventType": "PaymentScheduled"
        },
        {
            "createdDate": "2019-03-04T16:52:54.6570655Z",
            "effectiveDate": "2017-07-21T00:00:00+00:00",
            "eventId": 0,
            "eventType": "Created"
        }
    ],
    "lease": {
        "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0853",
        "maturityDate": "2018-07-21T00:00:00+00:00",
        "monthlyPaymentAmount": 25,
        "startDate": "2017-07-21T00:00:00+00:00"
    },
    "status": "Outstanding",
    "totalPaid": 25,
    "totalScheduled": 25,
    "updatedDate": "2019-03-04T16:55:22.8958666Z"
}
```
> Note that the payments are effective on the payment date.

Next, let's undo the `PaymentReceived` event pretending that the
payment never actual made it into our bank account because there was some
operational error.
```shell
curl -X DELETE \
  http://localhost:8080/lease/d290f1ee-6c54-4b01-90e6-d701748f0853/2
```
> `2` is the `eventId` of the `PaymentReceived` event.

```json
{
    "amountDue": 25,
    "createdDate": "2019-03-04T16:52:54.6570655Z",
    "events": [
        {
            "createdDate": "2019-03-04T16:53:32.8943226Z",
            "effectiveDate": "2017-07-22T00:00:00+00:00",
            "eventId": 1,
            "eventType": "PaymentScheduled"
        },
        {
            "createdDate": "2019-03-04T16:52:54.6570655Z",
            "effectiveDate": "2017-07-21T00:00:00+00:00",
            "eventId": 0,
            "eventType": "Created"
        }
    ],
    "lease": {
        "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0853",
        "maturityDate": "2018-07-21T00:00:00+00:00",
        "monthlyPaymentAmount": 25,
        "startDate": "2017-07-21T00:00:00+00:00"
    },
    "status": "Outstanding",
    "totalPaid": 0,
    "totalScheduled": 25,
    "updatedDate": "2019-03-04T16:53:32.8943226Z"
}
```
> Note that the `PaymentReceived` event is no longer in the `events` array. Also,
the `amountDue` has now gone back to `25`.

Lastly, let's terminate the lease.
```shell
curl -X DELETE \
  http://localhost:8080/lease/d290f1ee-6c54-4b01-90e6-d701748f0853
```
```json
{
    "amountDue": 25,
    "createdDate": "2019-03-04T16:52:54.6570655Z",
    "events": [
        {
            "createdDate": "2019-03-04T17:03:02.3684812Z",
            "effectiveDate": "2019-03-04T17:03:02.3655135Z",
            "eventId": 4,
            "eventType": "Terminated"
        },
        {
            "createdDate": "2019-03-04T16:53:32.8943226Z",
            "effectiveDate": "2017-07-22T00:00:00+00:00",
            "eventId": 1,
            "eventType": "PaymentScheduled"
        },
        {
            "createdDate": "2019-03-04T16:52:54.6570655Z",
            "effectiveDate": "2017-07-21T00:00:00+00:00",
            "eventId": 0,
            "eventType": "Created"
        }
    ],
    "lease": {
        "leaseId": "d290f1ee-6c54-4b01-90e6-d701748f0853",
        "maturityDate": "2018-07-21T00:00:00+00:00",
        "monthlyPaymentAmount": 25,
        "startDate": "2017-07-21T00:00:00+00:00"
    },
    "status": "Terminated",
    "totalPaid": 0,
    "totalScheduled": 25,
    "updatedDate": "2019-03-04T17:03:02.3684812Z"
}
```
> Note that now the `status` is `Terminated`.

Now for the best part. If you navigate to `http://localhost:2113` and
look at the stream `lease-d290f1ee6c544b0190e6d701748f0853` you can see all the
events that have happened. __Especially note the `Undid` event__. This should
make your auditors happy.

![eventstore](eventstore.png)

## Summary
In this post we covered the main functions and types used to handle a bi-temporal
domain, and how the Equinox library provides an easy way to handle the event
sourcing logic. Using this approach we have the flexibility to apply events
retroactively while maintaining an immutable log of all the events that have
occurred. There is a lot of other pieces to the complete application, so please
add a comment if you have any questions or think I could be doing something better! :smile:

## Resources
- [Equinox GitHub](https://github.com/jet/equinox)
- [Event Sourcing Basics](https://eventstore.org/docs/event-sourcing-basics/index.html)
- [12 Things You Should Know About Event Sourcing](http://blog.leifbattermann.de/2017/04/21/12-things-you-should-know-about-event-sourcing/)
