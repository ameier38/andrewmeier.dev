---
layout: post
title:  Dev Cluster
cover: /assets/images/dev-cluster/dev-cluster.png
permalink: dev-cluster
date: 2020-02-29 07:30:00 -0400
updated: 2020-02-29 07:30:00 -0400
categories: 
  - Kubernetes
  - Cluster
  - Ubuntu
  - Linux
comments: true
---

[Kubernetes](https://kubernetes.io/) is a great tool for managing workloads, which
could be a web server, chron job, API, etc. For the unfamiliar, Kubernetes
(or k8s for short) is a container (read: virtual machine) orchestration system
which makes it easy to securely and reliably run services.
To run Kubernetes you need physical servers, which must be purchased,
either by renting from your favorite cloud provider (AWS, Azure, GCP, etc.)
or buying and running your own.

Running Kubernetes in the cloud has become much easier in the last couple years
with tools such as AWS EKS, GCP GKE, or Azure AKS among others. While they make
it easy, it does not come without cost, and depending on how many workloads
you need to run, it can really add up.

For production services you can justify the cost as you are (hopefully :pray:)
getting paid by the customers who are using those services. For development
or for learning, paying for a cluster to just to test things is harder to justify.
But what if you could run a cluster for free[^1] :thinking:?

With great tools like [k3s](https://github.com/rancher/k3s) and
[k3sup](https://github.com/alexellis/k3sup), creating your own Kubernetes
cluster is easier than ever. In this post will cover:
1. Setting up servers (in this case a few Raspberry Pis),
2. Setting up secure SSH to connect to the servers,
3. Installing required tools for creating the Kubernetes cluster,
4. Creating the Kubernetes cluster,
5. 

## Materials
- Computers (preferably PC) with at least 2GB available on hard drive.
We started with an old [Dell OptiPlex Micro](https://www.dell.com/en-us/work/shop/desktops-n-workstations/3070-micro/spd/optiplex-3070-micro).
- Flash Drive (at least 4GB)
- [Ethernet cable](https://smile.amazon.com/AmazonBasics-RJ45-Cat-6-Ethernet-Patch-Cable-3-Feet-0-9-Meters/dp/B00N2VISLW/ref=sr_1_5?crid=3J6XSVDKUAV6Q&keywords=gigabit+ethernet+cable&qid=1583696671&sprefix=gigabit+ethernet%2Caps%2C231&sr=8-5)
- [Ethernet switch](https://www.amazon.com/NETGEAR-GS305-300PAS-Gigabit-Ethernet-Unmanaged/dp/B07S98YLHM/ref=dp_ob_title_ce)
- [Raspberry Pi 4B](https://smile.amazon.com/gp/product/B07TC2BK1X/ref=ppx_yo_dt_b_search_asin_title?ie=UTF8&psc=1)
- [microSD card](https://smile.amazon.com/SanDisk-Ultra-microSDHC-Class-SDSDQUA-032G-A11A/dp/B007JTKLEK/ref=sr_1_8?crid=3ORAK5EAGOHZX&keywords=sandisk+micro+sd+card&qid=1583695487&refinements=p_n_feature_two_browse-bin%3A6518304011&rnid=6518301011&s=electronics&sprefix=sandis%2Celectronics%2C193&sr=1-8)
with 5-8 ports (depending on how many servers you want).
- [USB to USB-C cable](https://smile.amazon.com/gp/product/B01ASXBY62/ref=ppx_yo_dt_b_asin_title_o00_s00?ie=UTF8&psc=1)

## Steps (Ubuntu)
1. Install [Rufus](https://rufus.ie/). On Windows you can use scoop:
    ```
    scoop install rufus
    ```
2. Download [Ubuntu Server 18.04](https://ubuntu.com/download/server).
3. Install Ubuntu onto a flash drive. Follow the instructions
[here](https://ubuntu.com/tutorials/tutorial-create-a-usb-stick-on-windows#1-overview).
4. Shut down the computer you plan on using as the Ubuntu Server.
5. Plug in the flash drive which now has Ubuntu loaded as bootable.
6. Turn on the computer, then repeatedly press the `F2` key in order to get to the
BIOS screen.
> This may be a different key depending on the computer.

## Raspberry Pi Setup

# Steps (Raspberry Pi)
1. Download the Raspberry Pi Imaging Tool and [follow the instructions](https://www.raspberrypi.org/documentation/installation/installing-images/README.md)
to flash the microSD card with Raspbian Lite.

    ![raspbian-lite](/assets/images/dev-cluster/raspbian-lite.png)

3. 

## Resources

[^1]: Or just really cheap.