using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Helper for executing commands and returning the correct result.
    /// </summary>
    public static class CommandExecutor
    {
        /// <summary>
        /// Executes a command and returns the result as an <see cref="ExecutionResult"/> instance.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="ExecutionResult"/> instance containing the outcome of executing the command.</returns>
        public static async Task<ActionResult<ExecutionResult>> ExecuteForHttp(this IExecutionEngine engine, CommandBase command)
        {
            engine.Logger.LogDebug($"Validating {command.GetType().Name} command");
            var errors = await engine.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(engine, command, errors);
                return new BadRequestObjectResult(new ExecutionResult
                {
                    ValidationErrors = errors
                });
            }

            engine.Logger.LogDebug($"Executing {command.GetType().Name} command");
            var result = await engine.ExecuteAsync(command);
            if (!result.WasSuccessful)
            {
                LogExecutionFailure(engine, command, result);
                return new ObjectResult(new ExecutionResult
                {
                    ExecutionErrors = result.ToErrors() ?? Array.Empty<CommandError>()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await engine.CommitAsync();
            engine.Logger.LogInformation($"Executed {command.GetType().Name} successfully");
            return new ExecutionResult();
        }
        private static void LogExecutionFailure(IExecutionEngine engine, CommandBase command, CommandResult? result)
        {
            engine.Logger.LogError($"Execution for {command.GetType().Name} failed execution");
            if ((result != null) && !string.IsNullOrEmpty(result.Error))
            {
                engine.Logger.LogError(result.Error);
            }
        }

        private static void LogValidationFailure(IExecutionEngine engine, CommandBase command, IEnumerable<CommandError> errors)
        {
            engine.Logger.LogWarning($"Execution for {command.GetType().Name} failed validation");
            foreach (var error in errors)
            {
                engine.Logger.LogWarning(error.Error);
            }
        }
    }
}
