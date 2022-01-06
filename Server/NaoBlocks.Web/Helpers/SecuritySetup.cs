using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NaoBlocks.Engine.Data;
using System.Text;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Allows configuring the security settings for the Web API.
    /// </summary>
    public static class SecuritySetup
    {
        /// <summary>
        /// Adds the JWT security services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the settings to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <exception cref="ApplicationException">Thrown when the security has not been set.</exception>
        public static void AddJwtSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Security")
                .Get<Configuration.Security>();
            if (string.IsNullOrWhiteSpace(settings?.JwtSecret))
            {
                throw new ApplicationException("Cannot initialise application - missing JWT secret in settings");
            }

            var key = Encoding.ASCII.GetBytes(settings.JwtSecret);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
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
                opts.AddPolicy("Robot", policy => policy.RequireRole(UserRole.Robot.ToString()));
                opts.AddPolicy("TeacherOrRobot", policy => policy.RequireRole(UserRole.Robot.ToString(), UserRole.Teacher.ToString(), UserRole.Administrator.ToString()));
            });
        }
    }
}