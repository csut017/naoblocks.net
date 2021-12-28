using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
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

        [Theory]
        [InlineData("version")]
        [InlineData("system/addresses")]
        [InlineData("system/addresses/connect.txt")]
        [InlineData("system/config")]
        public async Task GetApiMethodAllowsAnonymous(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/v1/{url}");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
        }

        [Theory]
        [InlineData("whoami")]
        public async Task GetApiMethodRequiresAuthentication(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/v1/{url}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("system/siteAddress")]
        public async Task PostApiMethodRequiresAuthentication(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.PostAsync(
                $"/api/v1/{url}",
                new StringContent(string.Empty));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}