using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
            var user = GenerateUser();
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    AddDatabaseToServices(services, user);
                });
            }).CreateClient();
            var request = new
            {
                role = "student",
                name = user.Name,
                password = DefaultPassword
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
            var robot = GenerateRobot();
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    AddDatabaseToServices(services, robot);
                });
            }).CreateClient();
            var request = new
            {
                role = "robot",
                name = robot.MachineName,
                password = DefaultPassword
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
