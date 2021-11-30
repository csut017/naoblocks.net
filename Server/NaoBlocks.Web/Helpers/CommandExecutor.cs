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

        /// <summary>
        /// Executes a command and transforms the result before returning it in an <see cref="ExecutionResult"/> instance.
        /// </summary>
        /// <typeparam name="TIn">The input type.</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        /// <param name="engine">The engine to use.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="mapper">The mapper to transforms the result.</param>
        /// <returns>A <see cref="ExecutionResult"/> instance containing the outcome of executing the command.</returns>
        public static async Task<ActionResult<ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(this IExecutionEngine engine, CommandBase command, Func<TIn?, TOut> mapper)
            where TIn : class
        {
            engine.Logger.LogDebug($"Validating {command.GetType().Name} command");
            var errors = await engine.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(engine, command, errors);
                return new BadRequestObjectResult(new ExecutionResult<TOut>
                {
                    ValidationErrors = errors
                });
            }

            engine.Logger.LogDebug($"Executing {command.GetType().Name} command");
            var rawResult = (await engine.ExecuteAsync(command));
            if (!rawResult.WasSuccessful)
            {
                LogExecutionFailure(engine, command, rawResult);
                return new ObjectResult(new ExecutionResult<TOut>
                {
                    ExecutionErrors = rawResult.ToErrors() ?? Array.Empty<CommandError>()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await engine.CommitAsync();
            engine.Logger.LogInformation($"Executed {command.GetType().Name} successfully");

            engine.Logger.LogDebug($"Mapping from {typeof(TIn).Name} to {typeof(TOut).Name}");
            var result = rawResult.As<TIn>();
            var output = mapper(result.Output);
            return ExecutionResult.New(output);
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
