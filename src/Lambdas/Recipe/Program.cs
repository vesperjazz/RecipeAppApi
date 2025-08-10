using Lambdas.Recipe.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration to load from appsettings.json and environment variables
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables override JSON settings

// Add AWS Lambda hosting for REST API Gateway
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

// Register services with dependency injection
builder.Services.AddScoped<IRecipeService, RecipeService>();

var app = builder.Build();

// Get the configurable welcome message from configuration
var welcomeMessage = builder.Configuration["AppSettings:WelcomeMessage"];

app.MapGet("/", () => welcomeMessage);

// Add new endpoints that use the injected service
app.MapGet("/recipes", async (IRecipeService recipeService) =>
{
    var recipes = await recipeService.GetRecipeNamesAsync();
    return Results.Ok(recipes);
});

app.MapGet("/welcome", async (IRecipeService recipeService) =>
{
    var message = await recipeService.GetWelcomeMessageAsync();
    return Results.Ok(new { message });
});

app.Run();
