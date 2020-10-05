namespace NaoBlocks.Core.Commands.Helpers
{
    static class CommandErrorHelper
    {
        public static CommandError GenerateError(this CommandBase command, string error)
        {
            return new CommandError(command.Number, error);
        }
    }
}
