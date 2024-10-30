

# Smart Meter Microservices

  

This project consists of two microservices that handle smart meter data processing and report generation.

  

## Microservices Overview

  

### Meter Service

A microservice responsible for handling meter readings and data processing.

  

### Report Service

A microservice that manages report generation, processing, and downloads, integrated with RabbitMQ for asynchronous processing.

  

## Technology Stack

  

- .NET 8.0

- Entity Framework Core

- RabbitMQ (for message queuing)

- MassTransit (for message bus)

- SQL Server

- Docker

- Swagger/OpenAPI

  

## Prerequisites

  

- .NET 8.0 SDK

- Docker and Docker Compose

- SQL Server

- RabbitMQ

  

## Configuration

  

### Environment Variables

The Report Service requires the following environment variables:

-  `RabbitMQ_Server`

-  `RabbitMQ_User`

-  `RabbitMQ_Password`

  

### Connection Strings

Both services require a SQL Server connection string in their configuration:

```json

{

	"ConnectionStrings": {

		"DefaultConnection": "your_connection_string_here"

	}

}

```

  

## Getting Started

  

1. Clone the repository:

```bash

git clone https://github.com/mehmetbergel/landis-smart-meter-project

```

  

2. Set up the environment variables for RabbitMQ.

  

3. Start the services using Docker Compose:

```bash

cd .\landis-smart-meter-project\ docker-compose  up

```

  

4. Access the services:

- Meter Service: `http://localhost:7126/swagger`

- Report Service: `http://localhost:7008/swagger`

  

## API Documentation

  

### Meter Service

Handles meter data processing with the following endpoints:

- Meter readings management

- Data processing operations

  

### Report Service

Manages report generation and downloads:

- Report generation requests

- Report status checking

- Report downloads

  

## Development

  

### Running Locally

  

1. Start the required services (SQL Server, RabbitMQ):

```bash

docker-compose  up  rabbitmq  sqlserver

```

  

2. Run the Meter Service:

```bash

cd  MeterService

dotnet  run

```

  

3. Run the Report Service:

```bash

cd  ReportService

dotnet  run

```

  

### Testing

  

Run tests for each service:

```bash

dotnet  test  MeterService.Tests

dotnet  test  ReportService.Tests

```

  

## Features

  

### Meter Service

- Meter reading processing

- Data validation

- CORS support

- Swagger documentation

  

### Report Service

- Asynchronous report generation

- Message queue integration

- Report download management

- CORS support

- Swagger documentation









# Smart Meter System UI

  

A web-based application for processing and reporting smart meter data. This project was generated with [Angular CLI](https://github.com/angular/angular-cli).

  

## Prerequisites

  

- Node.js (v18 or later)

- npm (Node Package Manager)

- Angular CLI (`npm install -g @angular/cli`)

  

## Installation

  

1. Clone the repository

2. Navigate to the project directory:

```bash

cd frontend/SmartMeterSystemUI

```

3. Install dependencies:

```bash

npm install

```

  

## Development Server

  

Run `ng serve` or `npm start` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

  

## Build

  

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

  

- Production build: `ng build`

- Development build: `ng build --configuration development`

  

## Features

  

- Smart meter data processing

- Data visualization

- Reporting system

- Material Design UI components

- Responsive layout
