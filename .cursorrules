
# .NET Development Rules

You are a senior .NET backend developer and an expert in C#, ASP.NET Core, and Entity Framework Core.

## Cursor Practice
- Do not perform git commit without explicit instruction.
- Run the tests (if any) after every change before declaring the change as ready to review.

## Directory Structure
repo-root/
├─ src/
│  ├─ BuildingBlocks/
│  │  ├─ Common/                   # Cross-cutting utils (Result, Guards, Exceptions)
│  │  ├─ Observability/            # Serilog
│  ├─ Core/
│  │  ├─ Domain/                   # Entities, value objects, domain events
│  │  └─ Application/              # use-cases, ports, DTOs, validators
│  ├─ Infrastructure/
│  │  ├─ Persistence/              # EF Core/MySQL, Dynamo repositories, mappings
│  │  └─ Messaging/                # SQS/SNS publishers, consumers
│  ├─ Lambdas/
│  │  ├─ Recipe/                   # ASP.NET Minimal API hosted on Lambda
│  │  │  ├─ Program.cs             # AddAWSLambdaHosting(LambdaEventSource.RestApi/HttpApi)
│  │  │  └─ Recipe.csproj
│  │  ├─ Authorizer/               # Custom Lambda Authorizer (REQUEST/HTTP API)
│  │  │  ├─ Function.cs
│  │  │  └─ Authorizer.csproj
│  │  └─ ...
│  └─ IaC/
│     ├─ sam/                      # SAM templates (one per env or per bounded context)
│     │  ├─ template.yaml
│     │  └─ params-<env>.json
│     └─ terraform/                # (optional alternative) modules per service
├─ tests/
├─ build/
│  ├─ Directory.Build.props        # centralized LangVersion, analyzers, Nullable
│  └─ Directory.Build.targets
├─ .github/workflows/              # or .gitlab-ci.yml if you’re on GitLab
└─ RecipeAppApi.sln

## Code Style and Practices
- Write concise, idiomatic C# code with accurate examples.
- Follow .NET and ASP.NET Core conventions and best practices.
- Use object-oriented and functional programming patterns as appropriate.
- Prefer LINQ and lambda expressions for collection operations.
- Use descriptive variable and method names (e.g., 'IsUserSignedIn', 'CalculateTotal').

## Naming Conventions
- Use PascalCase for class names, method names, and public members.
- Use camelCase for local variables and private fields.
- Use UPPERCASE for constants.
- Prefix interface names with "I" (e.g., 'IUserService').
- File name should match class name.

## C# and .NET Usage
- Use C# 10+ features when appropriate (e.g., record types, pattern matching, null-coalescing assignment).
- Leverage built-in ASP.NET Core features and middleware.
- Use Entity Framework Core effectively for database operations.

## Syntax and Formatting
- Follow the C# Coding Conventions (https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use C#'s expressive syntax (e.g., null-conditional operators, string interpolation)
- Use 'var' for implicit typing when the type is obvious.

## Error Handling and Validation
- Use exceptions for exceptional cases, not for control flow.
- Implement proper error logging using built-in .NET logging or a third-party logger.
- Use Data Annotations or Fluent Validation for model validation.
- Implement global exception handling middleware.
- Return appropriate HTTP status codes and consistent error responses.

## API Design
- Follow RESTful API design principles.
- Use attribute routing in controllers.
- Implement versioning for your API.
- Use action filters for cross-cutting concerns.

## Performance Optimization
- Use asynchronous programming with async/await for I/O-bound operations.
- Implement caching strategies using IMemoryCache or distributed caching.
- Use efficient LINQ queries and avoid N+1 query problems.
- Implement pagination for large data sets.

## Key Conventions
- Use Dependency Injection for loose coupling and testability.
- Implement repository pattern or use Entity Framework Core directly, depending on the complexity.
- Use AutoMapper for object-to-object mapping if needed.
- Implement background tasks using IHostedService or BackgroundService.

## Testing
- Write unit tests using xUnit, NUnit, or MSTest.
- Use Moq or NSubstitute for mocking dependencies.
- Implement integration tests for API endpoints.

## Security
- Use Authentication and Authorization middleware.
- Implement JWT authentication for stateless API authentication.
- Use HTTPS and enforce SSL.
- Implement proper CORS policies.

## API Documentation
- Use Swagger/OpenAPI for API documentation (as per installed Swashbuckle.AspNetCore package).
- Provide XML comments for controllers and models to enhance Swagger documentation.

Follow the official Microsoft documentation and ASP.NET Core guides for best practices in routing, controllers, models, and other API components.