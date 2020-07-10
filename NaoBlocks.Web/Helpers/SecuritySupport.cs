using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NaoBlocks.Core.Models;
using System;
using System.Text;

namespace NaoBlocks.Web.Helpers
{
    public static class SecuritySupport
    {
        public static void AddJwtSecurity(this IServiceCollection services, AppSettings appSettings)
        {
            if (appSettings.JwtSecret == null) throw new ApplicationException("Cannot initialise application - missing JWT secret in settings");
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
                opts.AddPolicy("Robot", policy => policy.RequireRole(UserRole.Robot.ToString()));
                opts.AddPolicy("TeacherOrRobot", policy => policy.RequireRole(UserRole.Robot.ToString(), UserRole.Teacher.ToString(), UserRole.Administrator.ToString()));
            });
        }
    }
}