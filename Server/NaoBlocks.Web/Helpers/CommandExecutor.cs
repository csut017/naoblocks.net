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
            var commandName = command.GetType().Name;
            engine.Logger.LogDebug("Validating {command} command", commandName);
            var errors = await engine.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(engine, command, errors);
                return new BadRequestObjectResult(new ExecutionResult
                {
                    ValidationErrors = errors
                });
            }

            engine.Logger.LogDebug("Executing {command} command", commandName);
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
            engine.Logger.LogInformation("Executed {command} successfully", commandName);
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
        public static async Task<ActionResult<ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(this IExecutionEngine engine, CommandBase command, Func<TIn, TOut> mapper)
            where TIn : class
        {
            var commandName = command.GetType().Name;
            engine.Logger.LogDebug("Validating {command} command", commandName);
            var errors = await engine.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(engine, command, errors);
                return new BadRequestObjectResult(new ExecutionResult<TOut>
                {
                    ValidationErrors = errors
                });
            }

            engine.Logger.LogDebug("Executing {command} command", commandName);
            var rawResult = await engine.ExecuteAsync(command);
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
            engine.Logger.LogInformation("Executed {command} successfully", commandName);

            engine.Logger.LogDebug("Mapping from {input} to {output}", typeof(TIn).Name, typeof(TOut).Name);
            var result = rawResult.As<TIn>();
            if (result.Output == null) return new ExecutionResult<TOut>();
            var output = mapper(result.Output);
            return ExecutionResult.New(output);
        }

        /// <summary>
        /// Logs all validation errors.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <param name="command">The command raising the errors.</param>
        /// <param name="result">The result containing the errors.</param>
        public static void LogExecutionFailure(this IExecutionEngine engine, CommandBase command, CommandResult? result)
        {
            engine.Logger.LogError("Execution for {command} failed execution", command.GetType().Name);
            if ((result != null) && !string.IsNullOrEmpty(result.Error))
            {
                engine.Logger.LogError(result.Error);
            }
        }

        /// <summary>
        /// Logs all validation errors.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <param name="command">The command raising the errors.</param>
        /// <param name="errors">The errors to log.</param>
        public static void LogValidationFailure(this IExecutionEngine engine, CommandBase command, IEnumerable<CommandError> errors)
        {
            engine.Logger.LogWarning("Execution for {command} failed validation", command.GetType().Name);
            foreach (var error in errors)
            {
                engine.Logger.LogWarning(error.Error);
            }
        }
    }
}