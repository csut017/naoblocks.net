using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public static class CommandExecutor
    {
        public static async Task<ActionResult<ExecutionResult>> ExecuteForHttp(this ICommandManager commandManager, CommandBase command)
        {
            if (commandManager == null) throw new ArgumentNullException(nameof(commandManager));
            if (command == null) throw new ArgumentNullException(nameof(command));

            commandManager.Logger.LogDebug($"Validating {command.GetType().Name} command");
            var errors = await commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(commandManager, command, errors);
                return new BadRequestObjectResult(new ExecutionResult
                {
                    ValidationErrors = errors
                });
            }

            commandManager.Logger.LogDebug($"Executing {command.GetType().Name} command");
            var result = await commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                LogExecutionFailure(commandManager, command, result);
                return new ObjectResult(new ExecutionResult
                {
                    ExecutionErrors = result.ToErrors() ?? Array.Empty<CommandError>()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await commandManager.CommitAsync();
            commandManager.Logger.LogInformation($"Executed {command.GetType().Name} successfully");
            return new ExecutionResult();
        }

        private static void LogExecutionFailure(ICommandManager commandManager, CommandBase command, CommandResult? result)
        {
            commandManager.Logger.LogError($"Execution for {command.GetType().Name} failed execution");
            if ((result != null) && !string.IsNullOrEmpty(result.Error))
            {
                commandManager.Logger.LogError(result.Error);
            }
        }

        private static void LogValidationFailure(ICommandManager commandManager, CommandBase command, IEnumerable<CommandError> errors)
        {
            commandManager.Logger.LogWarning($"Execution for {command.GetType().Name} failed validation");
            foreach (var error in errors)
            {
                commandManager.Logger.LogWarning(error.Error);
            }
        }

        public static async Task<ActionResult<ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(this ICommandManager commandManager, CommandBase<TIn> command, Func<TIn?, TOut> mapper)
            where TIn : class
        {
            if (commandManager == null) throw new ArgumentNullException(nameof(commandManager));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            commandManager.Logger.LogDebug($"Validating {command.GetType().Name} command");
            var errors = await commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(commandManager, command, errors);
                return new BadRequestObjectResult(new ExecutionResult<TOut>
                {
                    ValidationErrors = errors
                });
            }

            commandManager.Logger.LogDebug($"Executing {command.GetType().Name} command");
            var rawResult = (await commandManager.ApplyAsync(command));
            if (!rawResult.WasSuccessful)
            {
                LogExecutionFailure(commandManager, command, rawResult);
                return new ObjectResult(new ExecutionResult<TOut>
                {
                    ExecutionErrors = rawResult.ToErrors() ?? Array.Empty<CommandError>()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await commandManager.CommitAsync();
            commandManager.Logger.LogInformation($"Executed {command.GetType().Name} successfully");

            commandManager.Logger.LogDebug($"Mapping from {typeof(TIn).Name} to {typeof(TOut).Name}");
            var result = rawResult.As<TIn>();
            var output = mapper(result.Output);
            return ExecutionResult.New(output);
        }

        public static async Task<ActionResult<ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(this ICommandManager commandManager, CommandBase<TIn> command, Func<TIn?, Task<TOut>> mapper)
            where TIn : class
        {
            if (commandManager == null) throw new ArgumentNullException(nameof(commandManager));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            commandManager.Logger.LogDebug($"Validating {command.GetType().Name} command");
            var errors = await commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                LogValidationFailure(commandManager, command, errors);
                return new BadRequestObjectResult(new ExecutionResult<TOut>
                {
                    ValidationErrors = errors
                });
            }

            commandManager.Logger.LogDebug($"Executing {command.GetType().Name} command");
            var rawResult = (await commandManager.ApplyAsync(command));
            if (!rawResult.WasSuccessful)
            {
                LogExecutionFailure(commandManager, command, rawResult);
                return new ObjectResult(new ExecutionResult<TOut>
                {
                    ExecutionErrors = rawResult.ToErrors() ?? Array.Empty<CommandError>()
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await commandManager.CommitAsync();
            commandManager.Logger.LogInformation($"Executed {command.GetType().Name} successfully");

            commandManager.Logger.LogDebug($"Mapping from {typeof(TIn).Name} to {typeof(TOut).Name}");
            var result = rawResult.As<TIn>();
            var output = await mapper(result.Output);
            return ExecutionResult.New(output);
        }
    }
}