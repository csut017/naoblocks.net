using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class SystemControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public SystemControllerTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task VersionRetrievesJson()
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/version");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<VersionInformation>(content);
            Assert.False(string.IsNullOrWhiteSpace(data?.Version));
            Assert.False(string.IsNullOrWhiteSpace(data?.Status));
        }

        [Fact]
        public async Task WhoAmIHandlesNonAuthenticatedRequest()
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/whoami");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}