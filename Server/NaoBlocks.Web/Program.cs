using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Database;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

using Angular = NaoBlocks.Definitions.Angular;
using Configuration = NaoBlocks.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddHealthChecks();

// Register the configuration settings
builder.Services.Configure<RavenDbConfiguration>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<Configuration.Security>(
    builder.Configuration.GetSection("Security"));

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

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    opts.AddSecurityDefinition("Bearer", securitySchema);
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
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
        return RavenDbDatabase.New(logger, config?.Value, env.ContentRootPath).Result;
    });
builder.Services.AddScoped<IDatabaseSession>(services =>
{
    var database = services.GetRequiredService<IDatabase>();
    return database.StartSession();
});

// Add security
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPrincipal>(s =>
    s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());

// Add the application services
builder.Services.AddScoped<IExecutionEngine, ExecutionEngine>();
builder.Services.AddSingleton<IHub, LocalHub>();
ClientAddressList.Initialise();
var uiManager = new UiManager();
uiManager.Register<Angular.Definition>("angular");
builder.Services.AddSingleton(uiManager);
builder.Services.AddTransient<IEngineFactory, DefaultEngineFactory>();
builder.Services.AddTransient<IMessageProcessor, MessageProcessor>();

// Configure the application
var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    ClientAddressList.Add("http://localhost:5000", "https://localhost:5001");
}
else
{
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");
app.UseAngularUI();
app.UseRouting();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
/// Make the Program class explicit so it can be accessed by tests
/// </summary>
#pragma warning disable CA1050 // We need to expore Program as a class so it can be used in the integration tests
public partial class Program
#pragma warning restore CA1050
{ }