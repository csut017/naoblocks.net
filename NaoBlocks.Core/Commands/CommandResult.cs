namespace NaoBlocks.Core.Commands
{
    public class CommandResult
    {
        public CommandResult(int number, string error = null)
        {
            this.Number = number;
            this.Error = error;
        }

        public string Error { get; set; }

        public int Number { get; private set; }

        public bool WasSuccessful
        {
            get { return string.IsNullOrEmpty(this.Error); }
        }
    }
}