using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Lambdas.Users.Services;
using BuildingBlocks.Observability;
using Lambdas.Users;
using Lambdas.Users.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
builder.Host.ConfigureSerilog(builder.Configuration);

// Configure configuration to load from appsettings.json and environment variables
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables override JSON settings

// Add AWS Lambda hosting for REST API Gateway
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// Add database context
builder.Services.AddDatabase(builder.Configuration);

// Register services with dependency injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IJwtService, JwtService>();

// Add Swagger/OpenAPI for local development documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Users API", Version = "v1" });
    
    // Include XML comments if they exist
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure Swagger for development environment only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users API v1");
        c.RoutePrefix = "swagger";
    });
}

// Get the configurable welcome message from configuration
var welcomeMessage = builder.Configuration["AppSettings:WelcomeMessage"];

app.MapGet("/", () => welcomeMessage)
   .WithName("GetWelcome")
   .WithSummary("Gets the welcome message from configuration")
   .WithDescription("Returns the configured welcome message from appsettings");

// Add new endpoints that use the injected service
app.MapGet("/users", async (IUserService userService, ILogger<Program> logger) =>
{
    var users = await userService.GetUserNamesAsync();
    logger.LogInformation("Hey, here's some logging!");
    return Results.Ok(users);
})
.WithName("GetUsers")
.WithSummary("Gets all user names")
.WithDescription("Returns a list of user names from the user service");

app.MapGet("/welcome", async (IUserService userService) =>
{
    var message = await userService.GetWelcomeMessageAsync();
    return Results.Ok(new { message });
})
.WithName("GetServiceWelcome")
.WithSummary("Gets a welcome message from the user service")
.WithDescription("Returns a welcome message from the injected user service");

// Sign Up endpoint
app.MapPost("/auth/signup", async (SignUpRequest request, IUserService userService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Sign up request received for username: {Username}", request.Username);
        
        var response = await userService.SignUpAsync(request);
        
        logger.LogInformation("User successfully registered with ID: {UserId}", response.Id);
        
        return Results.Created($"/users/{response.Id}", response);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning("Sign up failed due to business rule violation: {Message}", ex.Message);
        
        var errorResponse = new ErrorResponse
        {
            Message = ex.Message,
            Details = "The requested username or email is already taken."
        };
        
        return Results.Conflict(errorResponse);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during sign up for username: {Username}", request.Username);
        
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An unexpected error occurred during registration. Please try again later or contact support if the problem persists.",
            statusCode: 500);
    }
})
.WithName("SignUp")
.WithSummary("Registers a new user")
.WithDescription("Creates a new user account with the provided username, email, and password")
.Accepts<SignUpRequest>("application/json")
.Produces<SignUpResponse>(201)
.Produces<ErrorResponse>(400)
.Produces<ErrorResponse>(409)
.Produces<ErrorResponse>(500);

// Sign In endpoint
app.MapPost("/auth/signin", async (SignInRequest request, IUserService userService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Sign in request received for username: {Username}", request.Username);
        
        var response = await userService.SignInAsync(request);
        
        logger.LogInformation("User successfully signed in with ID: {UserId}", response.Id);
        
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning("Sign in failed due to invalid credentials: {Message}", ex.Message);
        
        var errorResponse = new ErrorResponse
        {
            Message = ex.Message,
            Details = "The provided username or password is incorrect."
        };
        
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during sign in for username: {Username}", request.Username);
        
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An unexpected error occurred during authentication. Please try again later or contact support if the problem persists.",
            statusCode: 500);
    }
})
.WithName("SignIn")
.WithSummary("Authenticates a user")
.WithDescription("Authenticates a user with the provided username and password, returning a JWT access token")
.Accepts<SignInRequest>("application/json")
.Produces<SignInResponse>(200)
.Produces<ErrorResponse>(401)
.Produces<ErrorResponse>(500);

app.Run();
