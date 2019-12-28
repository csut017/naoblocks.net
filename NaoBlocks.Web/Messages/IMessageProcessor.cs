using NaoBlocks.Web.Communications;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Messages
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(ClientConnection client, ClientMessage message);
    }
}