using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Raven.TestDriver;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class StoreDefaultAddressTests : DatabaseHelper
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new StoreDefaultAddress();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Address is required for storing a default site address" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new StoreDefaultAddress
            {
                Address = "Somewhere"
            };
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteAddsNewSystemValues()
        {
            var command = new StoreDefaultAddress
            {
                Address = "Somewhere"
            };
            using var store = InitialiseDatabase();
            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var values = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.NotNull(values);
            Assert.Equal("Somewhere", values!.DefaultAddress);
        }
        [Fact]
        public async Task ExecuteUpdatesExistingValues()
        {
            var command = new StoreDefaultAddress
            {
                Address = "Somewhere"
            };
            using var store = InitialiseDatabase(new SystemValues { DefaultAddress = "Nowhere" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful, "Command was not successful");
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var values = verifySession.Query<SystemValues>().FirstOrDefault();
            Assert.NotNull(values);
            Assert.Equal("Somewhere", values!.DefaultAddress);
        }
    }
}
