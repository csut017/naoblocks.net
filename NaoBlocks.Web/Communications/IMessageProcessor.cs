using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(ClientConnection client, ClientMessage message);
    }
}