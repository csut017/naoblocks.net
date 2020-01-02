using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications.Messages
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(ClientConnection client, ClientMessage message);
    }
}