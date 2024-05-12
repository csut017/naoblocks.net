using Esprima;
using NaoBlocks.Common;

namespace NaoBlocks.Engine;

/// <summary>
/// Static helper for checking the incoming JavaScript is valid.
/// </summary>
public static class JavaScriptChecker
{
    private static Lazy<JavaScriptParser> parser = new(() => new JavaScriptParser());

    /// <summary>
    /// Checks that the incoming javascript is valid.
    /// </summary>
    /// <param name="errorMessage">The base error message for invalid JavaScript.</param>
    /// <param name="javascript">The JavaScript to check.</param>
    /// <param name="errors">The commands structure to append the error details to.</param>
    /// <returns><c>true</c> if the JavaScript is valid; <c>false</c> otherwise.</returns>
    public static bool Check(string errorMessage, string javascript, List<CommandError> errors)
    {
        try
        {
            var cleaned = javascript.Replace(@"\\", @"\");
            parser.Value.ParseScript(cleaned);
            return true;
        }
        catch (ParserException err)
        {
            var commandError = new CommandError(0, errorMessage)
            {
                Details = $"Attempted to parse \"{javascript}\", received {err.Error}"
            };
            errors.Add(
                commandError);
            return false;
        }
    }
}