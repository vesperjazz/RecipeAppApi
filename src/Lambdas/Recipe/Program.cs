using Lambdas.Recipe.Services;
using BuildingBlocks.Observability;

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

app.Run();
