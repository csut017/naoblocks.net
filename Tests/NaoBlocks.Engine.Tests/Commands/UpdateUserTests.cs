using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using Raven.TestDriver;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class UpdateUserTests : RavenTestDriver
    {
        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new UpdateUser();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Current name is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new UpdateUser
            {
                Role = UserRole.Student,
                CurrentName = "Bob"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new User { Name = "Bob", Role = UserRole.Student });
                initSession.SaveChanges();
            }
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidateHashesPassword()
        {
            var command = new UpdateUser
            {
                Role = UserRole.Student,
                CurrentName = "Bob",
                Password = "1234"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.Null(command.Password);
            Assert.NotNull(command.HashedPassword);
            Assert.True(command.HashedPassword!.Verify("1234"));
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new UpdateUser
            {
                Role = UserRole.Student,
                CurrentName = "Bob"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Student Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreFailsIfUserIsMissing()
        {
            var command = new UpdateUser
            {
                Role = UserRole.Student,
                CurrentName = "Bob"
            };
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Equal(new[] { "Student Bob does not exist" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task RestoreReloadsUser()
        {
            var command = new UpdateUser
            {
                Role = UserRole.Student,
                CurrentName = "Bob"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new User { Name = "Bob", Role = UserRole.Student });
                initSession.SaveChanges();
            }

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.RestoreAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ExecuteSavesUser()
        {
            var command = new UpdateUser
            {
                Name = "Bill",
                CurrentName = "Bob",
                HashedPassword = Password.New("7890")
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new User { Name = "Bob", Password = Password.New("1234") });
                initSession.SaveChanges();
            }

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var user = verifySession.Query<User>().First();
            Assert.Equal("Bill", user.Name);
            Assert.True(user.Password.Verify("7890"));
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new UpdateUser
            {
                Name = " Bill ",
                CurrentName = "Bob",
                HashedPassword = Password.New("7890")
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new User { Name = "Bob", Password = Password.New("1234") });
                initSession.SaveChanges();
            }

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var user = verifySession.Query<User>().First();
            Assert.Equal("Bill", user.Name);
        }

        [Fact]
        public async Task ExecuteUpdatesStudentDemographic()
        {
            var command = new UpdateUser
            {
                Name = " Bill ",
                CurrentName = "Bob",
                Role = UserRole.Student,
                Age = 10,
                Gender = "Female"
            };
            using var store = GetDocumentStore();
            using (var initSession = store.OpenSession())
            {
                initSession.Store(new User { 
                    Name = "Bob", 
                    Role = UserRole.Student, 
                    StudentDetails = new StudentDetails
                    {
                        Age = 9,
                        Gender = "Male"
                    }
                });
                initSession.SaveChanges();
            }

            using (var session = store.OpenAsyncSession())
            {
                var engine = new FakeEngine(session);
                await engine.RestoreAsync(command);
                var result = await engine.ExecuteAsync(command);
                Assert.True(result.WasSuccessful);
                await engine.CommitAsync();
            }

            using var verifySession = store.OpenSession();
            var user = verifySession.Query<User>().First();
            Assert.Equal(10, user.StudentDetails?.Age);
            Assert.Equal("Female", user.StudentDetails?.Gender);
        }

        [Fact]
        public async Task ExecuteChecksInitialState()
        {
            var command = new UpdateUser
            {
                Name = " Bob ",
                Role = UserRole.Teacher
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Unexpected error: Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync", result.Error);
        }
    }
}
