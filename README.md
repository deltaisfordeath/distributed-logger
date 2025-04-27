### Distributed Logging System

<!-- ABOUT THE PROJECT -->
## About The Project
This project was developed as my final project for Georgia Southwestern State University's CSCI 6220 - Distributed Operating Systems course. The project demonstrates a distributed logging system which enables the aggregation of logs from an arbitrary number of nodes to a single Log Collector.

The project involves two main components, a Log Collector which accepts logs from nodes via HTTP requests, and a Log Producer which submits logs to the Log Collector. The two projects share a model for the Log Message class.

This project was built using .NET Core 8

## Getting Started

This project requires .NET Core 8 and Postgres to run.

The Log Collector requires user secrets to be populated with a ```JwtSecret``` value for token generation, a default connection string for Postgres connection, and a default admin username and password for the identity seeder. See <a href="https://github.com/deltaisfordeath/distributed-logger/blob/main/LogCollector/LogCollector/secrets.json.example">user secrets sample</a> for user secrets formatting.

The Log Producer requires user secrets to be populated with a default connection string for Postgres, an admin username and password for the Log Producer webapp, and a "DistributedLogUser" username and password for a user account registered with the Log Collector webapp for making authenticated HTTP requests to the Log Collector. See <a href="https://github.com/deltaisfordeath/distributed-logger/blob/main/LogProducer/LogProducer/secrets.json.example">user secrets sample</a> for user secrets formatting. The Log Producer's <a href="https://github.com/deltaisfordeath/distributed-logger/blob/main/LogProducer/LogProducer/Services/DistributedLogService.cs">Distributed Log Service</a> must also have the correct address/port number for ```_authUrl``` and ```_logUrl``` for making requests to the Log Collector.

The Log Collector and Log Producer must then run concurrently to pass logs from producer to consumer via HTTP requests.