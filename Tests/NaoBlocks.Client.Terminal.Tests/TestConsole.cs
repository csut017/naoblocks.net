namespace NaoBlocks.Client.Terminal.Tests
{
    public class TestConsole
        : IConsole
    {
        private readonly List<string> messages = new();

        public string[] Output
        {
            get { return messages.ToArray(); }
        }

        public void WriteError(string message)
        {
            this.messages.Add($"ERROR: {message}");
        }

        public void WriteMessage(string message)
        {
            this.messages.Add($"INFO: {message}");
        }
    }
}