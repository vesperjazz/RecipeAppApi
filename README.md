# RecipeAppApi

A modern .NET 8 recipe management API built with Clean Architecture principles and designed to run on AWS Lambda.

## Architecture

This project follows Clean Architecture principles with the following structure:

```
src/
├── BuildingBlocks/          # Cross-cutting concerns
│   ├── Common/             # Shared utilities, Result pattern, Guards
│   └── Observability/      # Logging, metrics, tracing
├── Core/                   # Business logic
│   ├── Domain/            # Entities, value objects, domain events
│   └── Application/       # Use cases, ports, DTOs, validators
├── Infrastructure/         # External concerns
│   ├── Persistence/       # Database access, repositories
│   └── Messaging/         # Message queues, event publishing
└── Lambdas/               # AWS Lambda functions
    ├── Recipe/            # Main API Lambda
    └── Authorizer/        # Custom Lambda Authorizer
```

## Prerequisites

- .NET 8.0 SDK
- AWS CLI (for deployment)
- AWS SAM CLI (for local testing)

## Getting Started

### Build the Solution

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Local Development

```bash
cd src/Lambdas/Recipe
dotnet run
```

### Deploy to AWS

```bash
cd src/IaC/sam
sam build
sam deploy --config-env dev
```

## Project Structure

- **BuildingBlocks.Common**: Shared utilities, Result pattern, Guards, Exceptions
- **BuildingBlocks.Observability**: Serilog configuration, metrics, tracing
- **Core.Domain**: Recipe entities, value objects, domain events
- **Core.Application**: Use cases, ports, DTOs, validators
- **Infrastructure.Persistence**: Entity Framework Core, MySQL repositories
- **Infrastructure.Messaging**: SQS/SNS publishers and consumers
- **Lambdas.Recipe**: ASP.NET Core Minimal API hosted on Lambda
- **Lambdas.Authorizer**: Custom Lambda Authorizer for HTTP API

## Development Guidelines

- Follow C# coding conventions
- Use async/await for I/O operations
- Implement proper error handling and logging
- Write unit tests for business logic
- Use dependency injection for loose coupling

## CI/CD

The project includes GitHub Actions workflows for:
- Building and testing on push/PR
- Automatic deployment to dev environment
- AWS SAM deployment

## Contributing

1. Create a feature branch from `develop`
2. Make your changes following the coding standards
3. Add tests for new functionality
4. Submit a pull request

## License

Copyright (c) RecipeAppApi. All rights reserved.
