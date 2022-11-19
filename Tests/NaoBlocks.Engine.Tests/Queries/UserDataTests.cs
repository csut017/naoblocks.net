using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class UserDataTests : DatabaseHelper
    {
        [Fact]
        public async Task CheckForAnyCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.CheckForAnyAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task RetrieveByIdAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Id = "users/1" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.RetrieveByIdAsync("users/1");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.RetrieveByNameAsync("Mia");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrievePageAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.RetrievePageAsync(0, 10);
            Assert.Equal(1, result.Count);
            Assert.NotEmpty(result.Items ?? Array.Empty<User>());
            Assert.Equal(0, result.Page);
        }

        [Fact]
        public async Task RetrievePageAsyncWithTypeCallsDatabase()
        {
            using var store = InitialiseDatabase(new User { Name = "Mia", Role = UserRole.Student });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<UserData>(session);
            var result = await query.RetrievePageAsync(0, 10, UserRole.Student);
            Assert.Equal(1, result.Count);
            Assert.NotEmpty(result.Items ?? Array.Empty<User>());
            Assert.Equal(0, result.Page);
        }
    }
}