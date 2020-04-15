---
layout: post
title: Kafka Tutorial
cover: /assets/images/kafka-tutorial/cover.png
permalink: kafka-tutorial
date: 2018-12-23 12:00:00 -0400
categories: 
  - kafka
  - F#
comments: true
---

Set up a local Kafka cluster and walk through basic Kafka commands. This
follows the [Udemy Kafka Course](https://www.udemy.com/apache-kafka/).

## What is Kafka?
[Kafka](https://kafka.apache.org/intro) is a distributed streaming platform used to:
- Publish and subscribe to streams of records
- Store streams of records
- Process streams of records.

The above link has a more detailed description of Kafka and how it is used. A great
use case is using Kafka for the pipeline of events in an event-sourced application.

## Environment set up
Install [Docker](https://www.docker.com/). See instructions 
[here](https://andrewmeier.dev/win-dev#docker) for installing on Windows.

Install [Kubernetes](https://kubernetes.io/). See instructions
[here](https://andrewmeier.dev/win-dev#kubernetes) for installing on Windows.

Install [Helm](https://helm.sh/). See instructions
[here](https://andrewmeier.dev/win-dev#helm) for installing on Windows.

Add the [Bitnami repo](https://github.com/bitnami/charts).
```powershell
helm repo add bitnami https://charts.bitnami.com
helm repo update
```

Create a namespace for Kafka.
```powershell
kubectl create namespace kafka
kubectl config set-context docker-for-desktop --namespace kafka
```

Install the [Bitnami Kafka chart](https://github.com/bitnami/charts/tree/master/bitnami/kafka).
```powershell
helm install bitnami/kafka --name kafka --namespace kafka
```

Verify the installation.
```powershell
helm list
```
```powershell
helm status kafka
```

List the services (the service names are the domains used in the tutorial).
```powershell
kubectl get svc
NAME                       TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)                      AGE
kafka                      ClusterIP   10.99.206.57   <none>        9092/TCP                     56m
kafka-headless             ClusterIP   None           <none>        9092/TCP                     56m
kafka-zookeeper            ClusterIP   10.98.222.47   <none>        2181/TCP,2888/TCP,3888/TCP   56m
kafka-zookeeper-headless   ClusterIP   None           <none>        2181/TCP,2888/TCP,3888/TCP   56m
```

List the pods.
```powershell
kubectl get pods
NAME                READY     STATUS    RESTARTS   AGE
kafka-0             1/1       Running   1          1h
kafka-zookeeper-0   1/1       Running   0          1h
```

Install `telepresence` in order to proxy the Kafka service locally. We must use this so that the
IP addresses are resolved correctly when connecting to Kafka locally.
```shell
$ curl -s https://packagecloud.io/install/repositories/datawireio/telepresence/script.deb.sh | sudo bash
$ sudo apt install --no-install-recommends telepresence
```
> If you are using Windows then you must use the
[Windows Subsystem for Linux (WSL)](https://andrewmeier.dev/win-dev#windows-subsystem-for-linux).

Install .NET Core SDK.
```shell
$ wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
$ sudo apt-get install apt-transport-https
$ sudo apt-get update
$ sudo apt-get install dotnet-sdk-2.2
```
> If you are using Windows you must install in WSL so that you can use `telepresence`.

## Tutorial - CLI
This section will go through the steps to create a simple producer
and consumer using the Kafka command line scripts.

### Create a topic
Connect to the Kafka instance.
```powershell
kubectl exec -it kafka-0 bash 
```

Create a topic.
```shell
$ kafka-topics.sh --zookeeper kafka-zookeeper:2181 --topic logging.tutorial.main --create --partitions 3 --replication-factor 1
```
> `kafka-zookeeper:2181` is the address of the Zookeeper service retrieved from `kubectl get svc`.
Best practice for replication factor is > 1 but since we only have one broker we can only set `replication-factor=0`.
The topic naming is arbitrary and you can choose whatever topic name you like. See
[Kafka topic naming article](https://medium.com/@criccomini/how-to-paint-a-bike-shed-kafka-topic-naming-conventions-1b7259790073)
for some thoughts on how to name topics.

### Produce messages
Connect to the producer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main
```

Once the console launches you can type a message and press `Enter`.
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main
>hello
>world
```

To exit the producer console, press `Ctrl-C`.

### Produce messages with acknowledgement
Connect to the producer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main --producer-property acks=all
```
> Notice the additional `producer-property` argument. `acks=1` is the default which means
the leader broker will acknowledge the message. `acks=all` means that the leader
and the replica brokers will acknowledge the message. `acks=0` means that the producer
will not wait for acknowledgement.

Once the console launches you can type a message and press `Enter`.
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main --producer-property acks=all
>hello with ack
>world with ack
```

To exit the producer console, press `Ctrl-C`.

### Consume messages
Connect to the consumer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-consumer.sh --bootstrap-server kafka:9092 --topic logging.tutorial.main
```

Open a new terminal and connect to the producer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main
```

Write some messages in the producer console and you should see the messages
appear in the consumer console.

### Consume messages from beginning
Connect to the consumer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-consumer.sh --bootstrap-server kafka:9092 --topic logging.tutorial.main --from-beginning
```
> Note the additional `from-beginning` argument.

You should then see all the messages written to the `logging.tutorial.main` topic.

### Consume messages within a group
Connect to the consumer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-consumer.sh --bootstrap-server kafka:9092 --topic logging.tutorial.main --group my-app
```
> Note the additional `group` argument.

Open a new terminal and connect to another consumer console with the same group.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-consumer.sh --bootstrap-server kafka:9092 --topic logging.tutorial.main --group my-app
```

Open a new terminal and connect to the producer console.
```powershell
kubectl exec -it kafka-0 bash 
```
```shell
$ kafka-console-producer.sh --broker-list kafka:9092 --topic logging.tutorial.main
```

Write some messages in the producer console and you should see the messages
evenly split between the two consumer consoles.

## Tutorial
In this section we will create a producer and consumer in F#.
See the [source code](https://github.com/ameier38/kafka-beginners-course) for
for more details.

Clone the repo
```powershell
git clone https://github.com/ameier38/kafka-beginners-course.git
cd kafka-beginners-course
```

Install dependencies.
```powershell
.paket/paket.exe install
```
> If you are using OSX or Linux you will need to install [Mono](https://www.mono-project.com/).

Restore the project.
```powershell
dotnet restore
```

Compile the application.
```powershell
dotnet publish -o out
```
> This will compile the application and add the compiled assets into
a directory called `out`.

Start the consumer. We use telepresence to proxy the services locally.
```powershell
telepresence --run-shell --method inject-tcp
dotnet out/Tutorial.dll consumer kafka:9092 test_topic test_group
```
> This will start a consumer that will try to connect to a Kafka broker
at `kafka:9092` listening on the topic `test_topic` within the group `test_group`.
Using `telepresence` allows us to use the same DNS names in the Kubernetes
cluster.

Open a new terminal and start the producer.
```powershell
telepresence --run-shell --method inject-tcp
dotnet out/Tutorial.dll producer kafka:9092 test_topic test_key
```
> This will start a producer that will try to connect to a Kafka broker
at `kafka:9092` producing to the topic `test_topic` using the key `test_key`.

Enter messages into the producer terminal and you should see the messages
appear in the consumer terminal.

## Summary
In this post we covered:
- [Introduced Kafka](#what-is-kafka)
- [Set up environment](#environment-set-up)
- [Created a topic](#create-a-topic)
- [Produced messages](#produce-messages)
- [Consumed messages](#consume-messages)
- [Built F# produce and consumer](#tutorial)

Much thanks to the engineers at [Confluent](https://www.confluent.io/)
and [Jet.com](https://github.com/jet) for all the work on the Kafka and F#
libraries :raised_hands:!

## Additional Resources
- [Kafka homepage](https://kafka.apache.org/intro)
- [Udemy Kafka course](https://www.udemy.com/apache-kafka/)
- [Kafka topic naming](https://medium.com/@criccomini/how-to-paint-a-bike-shed-kafka-topic-naming-conventions-1b7259790073)
- [kafkacat](https://github.com/edenhill/kafkacat)
- [telepresence](https://www.telepresence.io/)
- [Connect to Kafka cluster in Kubernetes](https://medium.com/@valercara/connecting-to-a-kafka-cluster-running-in-kubernetes-7601ae3a87d6)
- [Install .NET Core SDK on Ubuntu](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-2.2.101)

I hope you enjoyed the post. If you run into any issues setting up the project leave a
comment and I can try to help debug :bug:.
