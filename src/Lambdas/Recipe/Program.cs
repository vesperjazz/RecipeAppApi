var builder = WebApplication.CreateBuilder(args);

// Configure configuration to load from appsettings.json and environment variables
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment variables override JSON settings

var app = builder.Build();

// Get the configurable welcome message from configuration
var welcomeMessage = builder.Configuration["AppSettings:WelcomeMessage"];

app.MapGet("/", () => welcomeMessage);

app.Run();
