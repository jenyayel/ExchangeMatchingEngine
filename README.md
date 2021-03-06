# Exchange Matching Engine

[![Build status](https://ci.appveyor.com/api/projects/status/mojrg54k455hkks5/branch/master?svg=true)](https://ci.appveyor.com/project/jenyayel/exchangematchingengine/branch/master)
[![Coverage Status](https://coveralls.io/repos/jenyayel/ExchangeMatchingEngine/badge.svg)](https://coveralls.io/r/jenyayel/ExchangeMatchingEngine)

Naive implementation of stocks exchange matching engine using [netmq](https://github.com/zeromq/netmq) and [actor model](http://en.wikipedia.org/wiki/Actor_model).

The API of engine exposed via two TCP sockets - one for sending commands and second for receiving events. 

The number of actors determinated based on number of logical processes on the machine. Here is a high level design of the application:

![High Level Design](https://raw.githubusercontent.com/jenyayel/ExchangeMatchingEngine/master/_docs/High%20Level%20Design.png "High Level Design")

See [unit test](https://github.com/jenyayel/ExchangeMatchingEngine/blob/master/EME.Tests/Application/AcceptanceTests.cs) for working example.
