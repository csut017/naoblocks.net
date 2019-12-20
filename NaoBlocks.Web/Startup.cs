using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Embedded;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseWebSockets();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.IgnoreNullValues = true;
                });

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            EmbeddedServer.Instance.StartServer(new ServerOptions
            {
                ServerUrl = "http://127.0.0.1:8088"
            });
            var store = EmbeddedServer.Instance.GetDocumentStore("NaoBlocks");
            store.Initialize();
            services.AddSingleton<IDocumentStore>(store);
            services.AddScoped(serviceProvider =>
            {
                return serviceProvider
                    .GetService<IDocumentStore>()
                    .OpenAsyncSession();
            });
            var key = Encoding.ASCII.GetBytes(appSettings.JwtSecret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
            });
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("Teacher", policy => policy.RequireRole(UserRole.Teacher.ToString(), UserRole.Administrator.ToString()));
                opts.AddPolicy("Administrator", policy => policy.RequireRole(UserRole.Administrator.ToString()));
            });

            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(s =>
                s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());

            services.AddScoped<ICommandManager, CommandManager>();
        }
    }
}