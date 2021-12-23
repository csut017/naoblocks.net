﻿using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Queries
{
    public class RobotTypeDataTests : DatabaseHelper
    {
        [Fact]
        public async Task RetrieveByNameAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrieveByNameAsync("Mihīni");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetrievePageAsyncCallsDatabase()
        {
            using var store = InitialiseDatabase(new RobotType { Name = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var query = InitialiseQuery<RobotTypeData>(session);
            var result = await query.RetrievePageAsync(0, 10);
            Assert.Equal(1, result.Count);
            Assert.NotEmpty(result.Items);
            Assert.Equal(0, result.Page);
        }
    }
}