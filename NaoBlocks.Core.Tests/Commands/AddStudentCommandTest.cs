using Moq;
using NaoBlocks.Core.Commands;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class AddStudentCommandTest
    {
        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddStudentCommand();
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Student name is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new AddStudentCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}