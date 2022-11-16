using Moq;
using NaoBlocks.Engine.Data;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public partial class ReportGeneratorTests
    {
        [Fact]
        public void SessionFailsIfNotInitialised()
        {
            var generator = new FakeGenerator();
            Assert.Throws<InvalidOperationException>(() => generator.Session);
        }

        [Fact]
        public void SessionReturnsSession()
        {
            var generator = new FakeGenerator();
            var session = new Mock<IDatabaseSession>();
            generator.InitialiseSession(session.Object);
            Assert.Same(session.Object, generator.Session);
        }

        [Fact]
        public void UserFailsIfNotInitialised()
        {
            var generator = new FakeGenerator();
            Assert.Throws<InvalidOperationException>(() => generator.User);
        }

        [Fact]
        public async Task UserReturnsUser()
        {
            var generator = new FakeGenerator();
            var user = new User();
            await generator.GenerateAsync(ReportFormat.Pdf, user);
            Assert.Same(user, generator.User);
            Assert.True(generator.IsCalled);
            Assert.Equal(ReportFormat.Pdf, generator.Format);
        }

        [Fact]
        public void RobotTypeFailsIfNotInitialised()
        {
            var generator = new FakeGenerator();
            Assert.Throws<InvalidOperationException>(() => generator.RobotType);
        }

        [Fact]
        public async Task RobotTypeReturnsRobotType()
        {
            var generator = new FakeGenerator();
            var robotType = new RobotType();
            await generator.GenerateAsync(ReportFormat.Excel, robotType);
            Assert.Same(robotType, generator.RobotType);
            Assert.True(generator.IsCalled);
            Assert.Equal(ReportFormat.Excel, generator.Format);
        }
    }
}
