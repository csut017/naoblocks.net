﻿using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class CommandExecutorTests
    {
        [Fact]
        public async Task ExecuteForHttpHandlesSuccess()
        {
            var engine = new FakeEngine();
            engine.ExpectCommand<FakeCommand>();
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected the execution to pass");
        }

        [Fact]
        public async Task ExecuteForHttpHandlesValidationFailure()
        {
            var engine = new FakeEngine
            {
                OnValidate = c => new CommandError[] { new CommandError(1, "oops") }
            };
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var httpResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Contains("oops", result.ValidationErrors.Select(e => e.Error));
            Assert.Empty(result.ExecutionErrors);
        }

        [Fact]
        public async Task ExecuteForHttpHandlesExecutionFailure()
        {
            var engine = new FakeEngine();
            engine.ExpectCommand<FakeCommand>(
                new CommandResult(1, "no go"));
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp(engine);
            var httpResult = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, httpResult.StatusCode);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Empty(result.ValidationErrors);
            Assert.Contains("no go", result.ExecutionErrors.Select(e => e.Error));
        }

        [Fact]
        public async Task ExecuteForHttpWithMapperHandlesSuccess()
        {
            var engine = new FakeEngine();
            engine.ExpectCommand<FakeCommand>(
                CommandResult.New(1, new SystemValues { DefaultAddress = "1234" }));
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp<SystemValues, string>(engine, sv => sv!.DefaultAddress);
            var result = Assert.IsType<ExecutionResult<string>>(response.Value);
            Assert.True(result.Successful, "Expected the execution to pass");
            Assert.Equal("1234", result.Output);
        }

        [Fact]
        public async Task ExecuteForHttpWithMapperHandlesValidationFailure()
        {
            var engine = new FakeEngine
            {
                OnValidate = c => new CommandError[] { new CommandError(1, "oops") }
            };
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp<SystemValues, string>(engine, sv => sv!.DefaultAddress);
            var httpResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            var result = Assert.IsType<ExecutionResult<string>>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Contains("oops", result.ValidationErrors.Select(e => e.Error));
            Assert.Empty(result.ExecutionErrors);
        }

        [Fact]
        public async Task ExecuteForHttpWithMapperHandlesExecutionFailure()
        {
            var engine = new FakeEngine();
            engine.ExpectCommand<FakeCommand>(
                new CommandResult(1, "no go"));
            var controller = new ControllerShell();
            var response = await controller.ExecuteForHttp<SystemValues, string>(engine, sv => sv!.DefaultAddress);
            var httpResult = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, httpResult.StatusCode);
            var result = Assert.IsType<ExecutionResult<string>>(httpResult.Value);
            Assert.False(result.Successful, "Expected the execution to fail");
            Assert.Empty(result.ValidationErrors);
            Assert.Contains("no go", result.ExecutionErrors.Select(e => e.Error));
        }

        private class ControllerShell : ControllerBase
        {
            public async Task<ActionResult<ExecutionResult>> ExecuteForHttp(IExecutionEngine engine)
            {
                var command = new FakeCommand();
                return await engine
                    .ExecuteForHttp(command)
                    .ConfigureAwait(false);
            }

            public async Task<ActionResult<ExecutionResult<TOut>>> ExecuteForHttp<TIn, TOut>(IExecutionEngine engine, Func<TIn, TOut> mapper)
                where TIn : class
            {
                var command = new FakeCommand();
                return await engine
                    .ExecuteForHttp(command, mapper)
                    .ConfigureAwait(false);
            }
        }
    }
}