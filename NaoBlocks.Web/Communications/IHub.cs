namespace NaoBlocks.Web.Communications
{
    public interface IHub
    {
        void AddClient(ClientConnection client);

        ClientConnection? GetClient(long id);

        void SendToAll(ClientMessage message);

        void SendToAll(ClientMessage message, ClientConnectionType clientType);
    }
}