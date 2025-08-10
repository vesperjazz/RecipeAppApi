# Project Requirements

## Bootstrap

1. **Configuration Setup**
   - Configure both `Lambdas.Recipe`, `Lambdas.User` and `Lambdas.Authorizer` to load settings from:
     - `appsettings.json`
     - Environment variables (override JSON where applicable).

2. **Lambda Hosting**
   - Configure `Lambdas.Recipe` and `Lambdas.User` to run using `AddAWSLambdaHosting(LambdaEventSource.RestApi)`
     for AWS REST API Gateway.

3. **Custom Authorizer**
   - Configure `Lambdas.Authorizer` to handle API Gateway request events
     and return authorization responses accordingly.

4. **Dependency Injection** 
   - Setup dependency injection for both `Lambdas.Recipe`, `Lambdas.Recipe` and `Lambdas.Authorizer`.
   - For Lambdas.Authorizer, inject a dummy ITokenValidator and TokenValidatorService with appropriate lifecycle.
   - For Lambdas.Users, inject a dummy IUserService and UserService with appropriate lifecycle.
   - For Lambdas.Recipe, inject a dummy IRecipeService and RecipeService with appropriate lifecycle.

5. **Logging**
   - Add Serilog with Console sink to both `Lambdas.Recipe`, `Lambdas.Recipe` and `Lambdas.Authorizer`.
   - Use structured logging and enrichers as needed.

6. **Swagger**
   - Integrate Swagger/OpenAPI in `Lambdas.Recipe` and `Lambdas.User` for local development documentation.

7. **Database Context**
   - Add an empty EF Core `DbContext` class to `Infrastructure.Persistence`.
   - Configure MySQL as the EF Core provider with connection string from configuration. 
   - Each lambda should have a database configuration class that refers to this `DbContext` and wired in the configuration pipeline.