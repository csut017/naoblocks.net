﻿using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class AddUserTests : DatabaseHelper
    {
        [Fact]
        public async Task ExecuteAddsStudentDemographics()
        {
            var command = new AddUser
            {
                Name = " Bob ",
                Role = UserRole.Student,
                HashedPassword = Password.New("1234"),
                Age = 7,
                Gender = "Male"
            };
            var engine = new FakeEngine();
            await engine.ExecuteAsync(command);
            var user = Assert.IsType<User>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.NotNull(user.StudentDetails);
            Assert.Equal(7, user.StudentDetails?.Age);
            Assert.Equal("Male", user.StudentDetails?.Gender);
        }

        [Fact]
        public async Task ExecuteAddsUser()
        {
            var command = new AddUser
            {
                Name = "Bob",
                Role = UserRole.Teacher,
                HashedPassword = Password.New("1234")
            };
            var engine = new FakeEngine();
            var result = await engine.ExecuteAsync(command);
            Assert.True(result.WasSuccessful);
            Assert.True(engine.DatabaseSession.StoreCalled);
            var user = Assert.IsType<User>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.Equal("Bob", user.Name);
            Assert.Equal(UserRole.Teacher, user.Role);
            Assert.True(user.Password.Verify("1234"));
        }

        [Fact]
        public async Task ExecuteTrimsWhitespaceFromName()
        {
            var command = new AddUser
            {
                Name = " Bob ",
                Role = UserRole.Teacher,
                HashedPassword = Password.New("1234")
            };
            var engine = new FakeEngine();
            await engine.ExecuteAsync(command);
            var user = Assert.IsType<User>(engine.DatabaseSession.GetLastModifiedEntity());
            Assert.Equal("Bob", user.Name);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234"
            };
            using var store = InitialiseDatabase(new User { Name = "Bob", Role = UserRole.Student });

            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Student with name Bob already exists" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForRobot()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234",
                Settings = new UserSettings
                {
                    RobotId = "karetao",
                }
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot karetao" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateChecksForRobotType()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234",
                Settings = new UserSettings
                {
                    RobotType = "Bobbot",
                }
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Unknown robot type Bobbot" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateHashesPassword()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            await engine.ValidateAsync(command);
            Assert.Null(command.Password);
            Assert.NotNull(command.HashedPassword);
            Assert.True(command.HashedPassword.Verify("1234"));
        }

        [Fact]
        public async Task ValidatePassesChecks()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234"
            };
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidatePassesChecksWithRobotAndType()
        {
            var command = new AddUser
            {
                Role = UserRole.Student,
                Name = "Bob",
                Password = "1234",
                Settings = new UserSettings
                {
                    RobotId = "karetao",
                    RobotType = "Bobbot"
                }
            };
            using var store = InitialiseDatabase(
                new Robot { MachineName = "karetao" },
                new RobotType { Name = "bobbot" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task ValidationChecksInputs()
        {
            var command = new AddUser();
            var engine = new FakeEngine();
            var errors = await engine.ValidateAsync(command);
            Assert.Equal(new[] { "Role is unknown or missing", "User name is required", "Password is required" }, FakeEngine.GetErrors(errors));
        }
    }
}