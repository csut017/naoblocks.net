﻿using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class UpdateUserCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new UpdateUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplyUpdatesUserName()
        {
            var data = new[]
            {
                new User { Name = "Old" }
            }; ;

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data.AsRavenQueryable());

            var command = new UpdateUserCommand { Name = "Bill", CurrentName = "Bob" };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            Assert.Equal("Bill", data[0].Name);
        }

        [Fact]
        public async Task ValidateHandlesExistingUser()
        {
            var data = new[]
            {
                new User { Name = "Old" }
            }.AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new UpdateUserCommand { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateHandlesMissingUser()
        {
            var data = new User[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new UpdateUserCommand { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher Bob does not exist"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new UpdateUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}