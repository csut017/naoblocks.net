namespace NaoBlocks.Core.Commands
{
    public class CommandError
    {
        public CommandError(int number, string error)
        {
            this.Number = number;
            this.Error = error;
        }

        public string Error { get; set; }

        public int Number { get; private set; }
    }
}