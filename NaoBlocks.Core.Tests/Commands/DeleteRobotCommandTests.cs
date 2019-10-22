﻿using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class DeleteRobotCommandTests
    {
        [Fact]
        public async Task ApplyDeletesRobot()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteRobotCommand { MachineName = "Bob" };
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.Delete(It.IsAny<Robot>()), Times.Once);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new DeleteRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteRobotCommand();
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Machine name is required for robot"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new DeleteRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}