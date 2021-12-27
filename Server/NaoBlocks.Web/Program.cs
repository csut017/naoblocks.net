using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Database;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});
builder.Services.AddHealthChecks();

// Register the configuration settings
builder.Services.Configure<RavenDbConfiguration>(
    builder.Configuration.GetSection("Database"));

// Add the controllers and documentation
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "NaoBlocks.NET API",
        Description = "Web API for interacting with the NaoBlocks system"
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Define the database services
builder.Services.AddSingleton<IDatabase>(services =>
    {
        var logger = services.GetRequiredService<ILogger<RavenDbDatabase>>();
        var config = services.GetService<IOptions<RavenDbConfiguration>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        return RavenDbDatabase.New(logger, config?.Value, env.ContentRootPath);
    });
builder.Services.AddScoped<IDatabaseSession>(services =>
{
    var database = services.GetRequiredService<IDatabase>();
    return database.StartSession();
});

// Add the application services
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddScoped<IExecutionEngine, ExecutionEngine>();
builder.Services.AddSingleton<IHub, LocalHub>();

var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Some basic configuration
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

/// <summary>
/// Make the Program class explicit so it can be accessed by tests
/// </summary>
public partial class Program
{ }