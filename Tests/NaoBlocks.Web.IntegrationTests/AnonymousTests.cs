using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NaoBlocks.Engine.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using Angular = NaoBlocks.Definitions.Angular;

namespace NaoBlocks.Web.IntegrationTests
{
    public class AnonymousTests
        : DatabaseHelper, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly ITestOutputHelper output;

        public AnonymousTests(ITestOutputHelper output, WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
            this.output = output;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/health")]
        [InlineData("/api/v1/version")]
        [InlineData("/api/v1/system/addresses")]
        [InlineData("/api/v1/system/addresses/connect.txt")]
        [InlineData("/api/v1/system/config")]
        public async Task GetApiMethodAllowsAnonymous(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
        }

        [Fact]
        public async Task RetrieveUiComponent()
        {
            // Arrange
            var definition = new UIDefinition
            {
                Name = "angular",
                Definition = new Angular.Definition()
            };
            var client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.Services.AddSingleton<ILoggerProvider>(
                        r => new XunitLoggerProvider(output, logging.UsesScopes()));

                });
                builder.ConfigureServices(services =>
                {
                    AddDatabaseToServices(services, definition);
                });
            }).CreateClient();

            // Act
            var url = "/api/v1/ui/angular/block_definitions";
            var response = await client.GetAsync(url);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.False(string.IsNullOrEmpty(content), "Expected some content");
        }

        [Theory]
        [InlineData("/api/v1/whoami")]
        [InlineData("/api/v1/clients/robot")]
        [InlineData("/api/v1/clients/mia/logs")]
        [InlineData("/api/v1/code/mia/1")]
        [InlineData("/api/v1/robots/karetao/logs/1")]
        public async Task GetApiMethodRequiresAuthentication(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("system/siteAddress")]
        [InlineData("code/compile")]
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