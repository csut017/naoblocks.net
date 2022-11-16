using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Database;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class DatabaseHelper : RavenTestDriver
    {
        public const string DefaultPassword = "test-1234";

        public static Robot GenerateRobot()
        {
            return new Robot
            {
                MachineName = "Karetao",
                Password = Password.New(DefaultPassword)
            };
        }

        public static User GenerateUser(UserRole role = UserRole.Student)
        {
            return new User
            {
                Role = role,
                Name = "Mia",
                Password = Password.New(DefaultPassword)
            };
        }

        public IDocumentStore InitialiseDatabase(params object[] entities)
        {
            var store = GetDocumentStore();
            IndexCreation.CreateIndexes(typeof(RavenDbDatabase).Assembly, store);
            using var session = store.OpenSession();
            foreach (var entity in entities)
            {
                session.Store(entity);
            }
            session.SaveChanges();
            WaitForIndexing(store);
            return store;
        }

        protected static async Task<string> GenerateSessionToken(User user, HttpClient client)
        {
            var request = new
            {
                role = "student",
                name = user.Name,
                password = DefaultPassword
            };
            var json = JsonConvert.SerializeObject(request);

            var response = await client.PostAsync(
                $"/api/v1/session",
                new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ExecutionResult<UserSessionResult>>(content);
            Assert.NotNull(result?.Output);
            var token = result!.Output!.Token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return token;
        }

        protected void AddDatabaseToServices(IServiceCollection services, params object[] entities)
        {
            services.AddSingleton<IDatabase>(services =>
            {
                var store = InitialiseDatabase(entities);
                var logger = services.GetRequiredService<ILogger<RavenDbDatabase>>();
                return new RavenDbDatabase(logger, store);
            });
        }
    }
}