![RSMQ: Redis Simple Message Queue for Node.js](https://img.webmart.de/rsmq_wide.png)

# Redis Simple Message Queue

A lightweight message queue for .NET that requires no dedicated queue server. Just a Redis server.
.NET implementation of https://github.com/smrchy/rsmq.

## implementation notes

Methods and properties are the same as the javascript lib, but follow the naming rules of .NET

## tests

Running the test project require a redis server.
Please pay attention of what redis server you configure on the test project as it erase the db 0 multiple time.