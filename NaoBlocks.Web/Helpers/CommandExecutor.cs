using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Core.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public static class CommandExecutor
    {
        public static async Task<ActionResult<Dtos.ExecutionResult>> ExecuteForHttp(this ICommandManager commandManager, CommandBase command)
        {
            if (commandManager == null) throw new ArgumentNullException(nameof(commandManager));

            var errors = await commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                return new BadRequestObjectResult(new Dtos.ExecutionResult
                {
                    ValidationErrors = errors
                });
            }

            var result = await commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                return new ObjectResult(new Dtos.ExecutionResult
                {
                    ExecutionErrors = new[] { result.Error }
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await commandManager.CommitAsync();
            return new Dtos.ExecutionResult();
        }

        public static async Task<ActionResult<Dtos.ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(this ICommandManager commandManager, CommandBase<TIn> command, Func<TIn, TOut> mapper)
            where TIn : class
        {
            if (commandManager == null) throw new ArgumentNullException(nameof(commandManager));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var errors = await commandManager.ValidateAsync(command);
            if (errors.Any())
            {
                return new BadRequestObjectResult(new Dtos.ExecutionResult<TOut>
                {
                    ValidationErrors = errors
                });
            }

            var result = await commandManager.ApplyAsync(command);
            if (!result.WasSuccessful)
            {
                return new ObjectResult(new Dtos.ExecutionResult<TOut>
                {
                    ExecutionErrors = new[] { result.Error }
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            await commandManager.CommitAsync();
            var output = mapper(command.Output);
            return Dtos.ExecutionResult.New(output);
        }
    }
}