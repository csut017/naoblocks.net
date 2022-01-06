using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class IntegrationSecurityTestHelper
        : DatabaseHelper, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public IntegrationSecurityTestHelper(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        protected async Task RunSecurityTest(UserRole role, string apiUrl, HttpStatusCode expectedCode)
        {
            // Arrange
            var user = GenerateUser(role);
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    AddDatabaseToServices(services, user);
                });
            }).CreateClient();
            await GenerateSessionToken(user, client);

            // Act
            var response = await client.GetAsync($"/api/v1/{apiUrl}");

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
            if (expectedCode != HttpStatusCode.OK) return;
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
            var json = JsonConvert.DeserializeObject(content);
            Assert.NotNull(json);
        }
    }
}
