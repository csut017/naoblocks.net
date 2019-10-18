﻿using Moq;
using NaoBlocks.Core.Commands;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests
{
    public class FakeCommandManager
        : ICommandManager
    {
        private Func<CommandBase, Task<CommandResult>> applyCommand;

        private Func<CommandBase, Task<IEnumerable<string>>> validateCommand;

        public FakeCommandManager()
        {
            this.Session = new Mock<IAsyncDocumentSession>().Object;
            this.SetupDefault();
        }

        public int CountOfApplyCalled { get; private set; }
        public int CountOfValidateCalled { get; private set; }
        public IAsyncDocumentSession Session { get; set; }

        public async Task<CommandResult> ApplyAsync(CommandBase command)
        {
            this.CountOfApplyCalled++;
            var result = await this.applyCommand(command);
            return result;
        }

        public Task CommitAsync()
        {
            return Task.CompletedTask;
        }

        public FakeCommandManager SetupApply(Func<CommandBase, Task<CommandResult>> command)
        {
            this.applyCommand = command;
            return this;
        }

        public FakeCommandManager SetupApplyError(string error)
        {
            this.applyCommand = c => Task.FromResult(new CommandResult(1, error));
            return this;
        }

        public FakeCommandManager SetupDefault()
        {
            this.applyCommand = async c => await c.ApplyAsync(this.Session);
            this.validateCommand = async c => await c.ValidateAsync(this.Session);
            return this;
        }

        public FakeCommandManager SetupDoNothing()
        {
            this.applyCommand = c => Task.FromResult(new CommandResult(1));
            this.validateCommand = c => Task.FromResult(new string[0].AsEnumerable());
            return this;
        }

        public FakeCommandManager SetupValidate(Func<CommandBase, Task<IEnumerable<string>>> command)
        {
            this.validateCommand = command;
            return this;
        }

        public FakeCommandManager SetupValidateErrors(params string[] errors)
        {
            this.validateCommand = c => Task.FromResult(errors.AsEnumerable());
            return this;
        }

        public async Task<IEnumerable<string>> ValidateAsync(CommandBase command)
        {
            this.CountOfValidateCalled++;
            return await this.validateCommand(command);
        }
    }
}