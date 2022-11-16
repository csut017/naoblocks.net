using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ImportToolboxTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsToolbox()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                Definition = "<toolbox>" +
                        "<category name=\"one\" order=\"2\" colour=\"1\">" +
                            "<block type=\"two\">three</block>" +
                        "</category>" +
                    "</toolbox>"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            var toolboxCount = robotType.Toolboxes.Count;
            Assert.Equal(1, toolboxCount);
            Assert.True(robotType.Toolboxes.First().IsDefault);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot"
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }

        [Fact]
        public async Task ExecuteLeavesDefaultToolbox()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                IsDefault = false,
                Definition = "<toolbox>" +
                        "<category name=\"one\" order=\"2\" colour=\"1\">" +
                            "<block type=\"two\">three</block>" +
                        "</category>" +
                    "</toolbox>"
            };
            var initialRobotType = new RobotType { Name = "Bobbot" };
            var initialToolbox = new Toolbox
            {
                Name = "Default",
                IsDefault = true,
            };
            initialToolbox.Categories.Add(new ToolboxCategory
            {
                Name = "existing"
            });
            initialRobotType.Toolboxes.Add(initialToolbox);
            using var store = InitialiseDatabase(initialRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            var toolboxCount = robotType.Toolboxes.Count;
            Assert.Equal(2, toolboxCount);
            Assert.Equal("Default",
                robotType.Toolboxes.SingleOrDefault(t => t.IsDefault)?.Name);
        }

        [Fact]
        public async Task ExecuteReplacesDefaultToolbox()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                IsDefault = true,
                Definition = "<toolbox>" +
                        "<category name=\"one\" order=\"2\" colour=\"1\">" +
                            "<block type=\"two\">three</block>" +
                        "</category>" +
                    "</toolbox>"
            };
            var initialRobotType = new RobotType { Name = "Bobbot" };
            var initialToolbox = new Toolbox
            {
                Name = "Default",
                IsDefault = true,
            };
            initialToolbox.Categories.Add(new ToolboxCategory
            {
                Name = "existing"
            });
            initialRobotType.Toolboxes.Add(initialToolbox);
            using var store = InitialiseDatabase(initialRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            var toolboxCount = robotType.Toolboxes.Count;
            Assert.Equal(2, toolboxCount);
            Assert.Equal("Testing",
                robotType.Toolboxes.SingleOrDefault(t => t.IsDefault)?.Name);
        }

        [Fact]
        public async Task ExecuteReplacesExistingToolbox()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                Definition = "<toolbox>" +
                        "<category name=\"one\" order=\"2\" colour=\"1\">" +
                            "<block type=\"two\">three</block>" +
                        "</category>" +
                    "</toolbox>"
            };
            var initialRobotType = new RobotType { Name = "Bobbot" };
            var initialToolbox = new Toolbox
            {
                Name = "Testing"
            };
            initialToolbox.Categories.Add(new ToolboxCategory
            {
                Name = "existing"
            });
            initialRobotType.Toolboxes.Add(initialToolbox);
            using var store = InitialiseDatabase(initialRobotType);

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var robotType = verifySession.Query<RobotType>().First();
            var toolboxCount = robotType.Toolboxes.Count;
            Assert.Equal(1, toolboxCount);
            Assert.Equal("one",
                robotType.Toolboxes.First().Categories.First().Name);
        }

        [Fact]
        public async Task ExecuteUsesCachedRobotType()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                Definition = "<toolbox>" +
                        "<category name=\"one\" order=\"2\" colour=\"1\">" +
                            "<block type=\"two\">three</block>" +
                        "</category>" +
                    "</toolbox>",
                IgnoreMissingRobotType = true
            };
            using var store = InitialiseDatabase(new RobotType());

            var cachedRobotType = new RobotType { Name = "Bobbot" };
            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                engine.DatabaseSession.CacheItem("Bobbot", cachedRobotType);
                await engine.RestoreAsync(command);    // Still need to restore as this command will prepare the document
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await session.StoreAsync(cachedRobotType);
                await engine.CommitAsync();
            }

            var toolboxCount = cachedRobotType!.Toolboxes.Count;
            Assert.Equal(1, toolboxCount);
            //var blockCount = cachedRobotType!.Toolbox.Select(tb => tb.Blocks.Count).Sum(tb => tb);
            //Assert.Equal(1, blockCount);
        }

        [Fact]
        public async Task RestoreFailsIfRobotTypeIsMissing()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                Definition = "<data/>"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsRobotType()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                Definition = "<data/>"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobotType()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                Definition = "<data/>",
                ToolboxName = "Testing",
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type Bobbot does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksToolboxName()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                Definition = "<data/>"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Toolbox name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksXML()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                Definition = "bad",
                ToolboxName = "Testing",
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unable to load definition" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateHandlesMissingRobotType()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                Definition = "<data/>",
                ToolboxName = "Testing",
                IgnoreMissingRobotType = true
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new ImportToolbox
            {
                RobotTypeName = "Bobbot",
                ToolboxName = "Testing",
                Definition = "<data/>"
            };
            using var store = InitialiseDatabase(new RobotType { Name = "Bobbot" });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new ImportToolbox();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Robot type name is required", "Toolbox name is required", "Definition is required" }, FakeEngine.GetErrors(errors));
        }
    }
}