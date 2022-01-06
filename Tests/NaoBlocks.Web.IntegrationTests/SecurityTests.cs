using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NaoBlocks.Web.IntegrationTests
{
    public class SecurityTests
        : DatabaseHelper, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly ITestOutputHelper output;

        public SecurityTests(ITestOutputHelper output, WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
            this.output = output;
        }

        [Theory]
        [ClassData(typeof(AdministratorSecurityTests))]
        [ClassData(typeof(RobotSecurityTests))]
        [ClassData(typeof(StudentSecurityTests))]
        [ClassData(typeof(TeacherSecurityTests))]
        public async Task Check(UserRole role, string url, HttpStatusCode expectedCode, bool checkForJson)
        {
            // Arrange
            var user = GenerateUser(role);
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.Services.AddSingleton<ILoggerProvider>(
                        r => new XunitLoggerProvider(output, UsesScopes(logging)));

                });
                builder.ConfigureServices(services =>
                {
                    AddDatabaseToServices(services, user);
                });
            }).CreateClient();
            await GenerateSessionToken(user, client);

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            if (expectedCode != HttpStatusCode.OK) return;
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
            if (!checkForJson) return;
            var json = JsonConvert.DeserializeObject(content);
            Assert.NotNull(json);
        }

        private static bool UsesScopes(ILoggingBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            // look for other host builders on this chain calling ConfigureLogging explicitly
            var options = serviceProvider.GetService<SimpleConsoleFormatterOptions>() ??
                          serviceProvider.GetService<JsonConsoleFormatterOptions>() ??
                          serviceProvider.GetService<ConsoleFormatterOptions>();

            if (options != default)
                return options.IncludeScopes;

            // look for other configuration sources
            // See: https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#set-log-level-by-command-line-environment-variables-and-other-configuration

            var config = serviceProvider.GetService<IConfigurationRoot>() ?? serviceProvider.GetService<IConfiguration>();
            var logging = config?.GetSection("Logging");
            if (logging == default)
                return false;

            var includeScopes = logging?.GetValue("Console:IncludeScopes", false);
            if (!includeScopes.Value)
                includeScopes = logging?.GetValue("IncludeScopes", false);

            return includeScopes.GetValueOrDefault(false);
        }
    }
}
