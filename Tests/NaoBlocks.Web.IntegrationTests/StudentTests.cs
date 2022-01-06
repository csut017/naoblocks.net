using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

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
        public async Task CanAccessAuthorizedApi(string url)
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
    }
}
