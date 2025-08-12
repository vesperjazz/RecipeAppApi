using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Lambdas.Recipe.Services;
using BuildingBlocks.Observability;
using Lambdas.Recipe;
using Lambdas.Recipe.DTOs;

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
builder.Services.AddScoped<IRecipeService, RecipeService>();

// Add Swagger/OpenAPI for local development documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Recipe API", Version = "v1" });
    
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe API v1");
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
app.MapGet("/recipes", async (IRecipeService recipeService, ILogger<Program> logger) =>
{
    var recipes = await recipeService.GetRecipeNamesAsync();
    logger.LogInformation("Hey, here's some logging!");
    return Results.Ok(recipes);
})
.WithName("GetRecipes")
.WithSummary("Gets all recipe names")
.WithDescription("Returns a list of recipe names from the recipe service");

app.MapGet("/welcome", async (IRecipeService recipeService) =>
{
    var message = await recipeService.GetWelcomeMessageAsync();
    return Results.Ok(new { message });
})
.WithName("GetServiceWelcome")
.WithSummary("Gets a welcome message from the recipe service")
.WithDescription("Returns a welcome message from the injected recipe service");

app.MapPost("/recipes", async (CreateRecipeRequest request, IRecipeService recipeService, ILogger<Program> logger, HttpContext httpContext) =>
{
    try
    {
        // For now, we'll use a hardcoded user ID. In a real application, this would come from authentication
        var createdByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        var createdRecipe = await recipeService.CreateRecipeAsync(request, createdByUserId);
        
        logger.LogInformation("Recipe created successfully: {RecipeId}", createdRecipe.Id);
        
        return Results.Created($"/recipes/{createdRecipe.Id}", createdRecipe);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating recipe");
        return Results.Problem("An error occurred while creating the recipe", statusCode: 500);
    }
})
.WithName("CreateRecipe")
.WithSummary("Creates a new recipe")
.WithDescription("Creates a new recipe with ingredients and steps")
.Accepts<CreateRecipeRequest>("application/json")
.Produces<CreateRecipeResponse>(201)
.ProducesProblem(500);

app.Run();
