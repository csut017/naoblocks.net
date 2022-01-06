using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class StudentTests
        : DatabaseHelper, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public StudentTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Theory]
        [InlineData("whoami")]
        public async Task GetCanAccessAuthorizedApi(string url)
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
            await GenerateSessionToken(user, client);

            // Act
            var response = await client.GetAsync($"/api/v1/{url}");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
        }

        [Theory]
        [InlineData("clients/robot")]
        [InlineData("clients/robot/logs")]
        public async Task GetFailsWithAuthorizedApi(string url)
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
            await GenerateSessionToken(user, client);

            // Act
            var response = await client.GetAsync($"/api/v1/{url}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
