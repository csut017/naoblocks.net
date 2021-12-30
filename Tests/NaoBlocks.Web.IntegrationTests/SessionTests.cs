using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Database;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.IntegrationTests
{
    public class SessionTests
        : DatabaseHelper, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public SessionTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task CanStartNewUserSession()
        {
            // Arrange
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IDatabase>(services =>
                    {
                        var store = InitialiseDatabase(
                            new Data.User
                            {
                                Name = "Mia",
                                Role = Data.UserRole.Student,
                                Password = Data.Password.New("mia-1234")
                            });
                        var logger = services.GetRequiredService<ILogger<RavenDbDatabase>>();
                        return new RavenDbDatabase(logger, store);
                    });
                });
            }).CreateClient();
            var request = new
            {
                role = "student",
                name = "Mia",
                password = "mia-1234"
            };
            var json = JsonConvert.SerializeObject(request);

            // Act
            var response = await client.PostAsync(
                $"/api/v1/session",
                new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ExecutionResult<UserSessionResult>>(content);
            Assert.Equal("Student", result?.Output?.Role);
            Assert.True((result?.Output?.TimeRemaining ?? -1) > 0, "No time remaining for session");
            Assert.False(string.IsNullOrEmpty(result?.Output?.Token), "Missing session token");
        }

        [Fact]
        public async Task CanStartNewRobotSession()
        {
            // Arrange
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IDatabase>(services =>
                    {
                        var store = InitialiseDatabase(
                            new Data.Robot
                            {
                                MachineName = "karetao",
                                Password = Data.Password.New("mia-1234")
                            });
                        var logger = services.GetRequiredService<ILogger<RavenDbDatabase>>();
                        return new RavenDbDatabase(logger, store);
                    });
                });
            }).CreateClient();
            var request = new
            {
                role = "robot",
                name = "karetao",
                password = "mia-1234"
            };
            var json = JsonConvert.SerializeObject(request);

            // Act
            var response = await client.PostAsync(
                $"/api/v1/session",
                new StringContent(json, Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ExecutionResult<UserSessionResult>>(content);
            Assert.Equal("Robot", result?.Output?.Role);
            Assert.True((result?.Output?.TimeRemaining ?? -1) > 0, "No time remaining for session");
            Assert.False(string.IsNullOrEmpty(result?.Output?.Token), "Missing session token");
        }
    }
}
