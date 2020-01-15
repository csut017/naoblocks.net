using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json.Serialization;

namespace NaoBlocks.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            PrintAppInformation(env, logger);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                var url = context.Request.Path.Value;
                if (!(url.StartsWith("/api", StringComparison.Ordinal) || url.Contains(".", StringComparison.Ordinal)))
                {
                    logger.LogInformation("Redirecting to default");
                    context.Request.Path = "/";
                }
                await next.Invoke();
            });

            var options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseWebSockets();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opts.JsonSerializerOptions.Converters.Add(new TutorialExerciseLineConverter());
                    opts.JsonSerializerOptions.IgnoreNullValues = true;
                });

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddHealthChecks();
            services.AddRavenDb(appSettings);
            services.AddJwtSecurity(appSettings);

            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(s =>
                s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());

            services.AddScoped<ICommandManager, CommandManager>();
            services.AddSingleton<IHub, Hub>();
            services.AddTransient<IMessageProcessor, MessageProcessor>();
        }

        private static void PrintAppInformation(IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var version = Assembly.GetEntryAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;
            logger.LogInformation($"Starting NaoBlocks.Net");
            logger.LogInformation($"=> version {version}");

            var environment = env.IsDevelopment() ? "DEVELOPMENT" : "PRODUCTION";
            logger.LogInformation($"=> {environment} environment");
        }
    }
}