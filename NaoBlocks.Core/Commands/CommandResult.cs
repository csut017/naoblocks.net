using System.Collections.Generic;

namespace NaoBlocks.Core.Commands
{
    public class CommandResult
    {
        public CommandResult(int number, string? error = null)
        {
            this.Number = number;
            this.Error = error;
        }

        public string? Error { get; set; }

        public int Number { get; private set; }

        public bool WasSuccessful
        {
            get { return string.IsNullOrEmpty(this.Error); }
        }

        public static CommandResult New(int number)
        {
            return new CommandResult(number);
        }

        public static CommandResult New<T>(int number, T value)
            where T : class
        {
            return new CommandResult<T>(number, value);
        }

        public CommandResult<T> As<T>()
            where T : class
        {
            return (CommandResult<T>)this;
        }

        public virtual IEnumerable<CommandError>? ToErrors()
        {
            if (string.IsNullOrEmpty(this.Error))
            {
                return null;
            }

            return new[] {
                new CommandError(this.Number, this.Error)
                };
        }
    }

    public class CommandResult<T> : CommandResult
        where T : class
    {
        public CommandResult(int number, string? error = null)
            : base(number, error)
        {
        }

        public CommandResult(int number, T output)
            : base(number)
        {
            this.Output = output;
        }

        public T? Output { get; set; }
    }
}