namespace NaoBlocks.Web.Communications
{
    public enum ClientMessageType
    {
        Unknown,
        Authenticate = 1,
        Authenticated = 2,
        StartProgram = 100,
        Error = 1000,
    }
}