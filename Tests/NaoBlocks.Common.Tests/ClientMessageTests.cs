using System.Text;
using Xunit;

namespace NaoBlocks.Common.Tests
{
    public class ClientMessageTests
    {
        [Fact]
        public void ConstructorInitialisesEmptyInstance()
        {
            var msg = new ClientMessage();
            Assert.Equal(ClientMessageType.Unknown, msg.Type);
            Assert.Empty(msg.Values);
            Assert.Null(msg.ConversationId);
        }

        [Fact]
        public void ConstructorExtractsObjectValues()
        {
            var msg = new ClientMessage(ClientMessageType.ProgramStarted, new
            {
                Item = "One",
                Second = (string?)null
            });
            Assert.Equal(ClientMessageType.ProgramStarted, msg.Type);
            Assert.Equal("One", Assert.Contains("Item", msg.Values));
            Assert.Equal(string.Empty, Assert.Contains("Second", msg.Values));
            Assert.Null(msg.ConversationId);
        }

        [Fact]
        public void ConstructorHandlesNullObject()
        {
            var msg = new ClientMessage(ClientMessageType.ProgramStarted, null);
            Assert.Equal(ClientMessageType.ProgramStarted, msg.Type);
            Assert.Empty(msg.Values);
            Assert.Null(msg.ConversationId);
        }

        [Fact]
        public void CloneCopiesAllValues()
        {
            var msg = new ClientMessage(ClientMessageType.Authenticate, new
            {
                Item = "One"
            })
            {
                ConversationId = 5
            };
            var clone = msg.Clone();
            Assert.Equal(ClientMessageType.Authenticate, clone.Type);
            Assert.Equal("One", Assert.Contains("Item", msg.Values));
            Assert.Equal(5, msg.ConversationId);
        }

        [Fact]
        public void ToArrayGeneratesBuffer()
        {
            var msg = new ClientMessage(ClientMessageType.RequestRobot, null);
            var buffer = msg.ToArray();
            var expected = Encoding.UTF8.GetBytes("{\"type\":11,\"values\":{}}");
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void FromArrayGeneratesInstance()
        {
            var buffer = Encoding.UTF8.GetBytes("{\"type\":11,\"values\":{}}");
            var msg = ClientMessage.FromArray(buffer);
            Assert.Equal(ClientMessageType.RequestRobot, msg.Type);
        }
    }
}
