using NaoBlocks.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    public class FakeCommand : CommandBase
    {
        public Exception? Error { get; set; }

        public bool ExecuteCalled { get; private set; }

        public bool ValidateCalled { get; private set; }

        public Func<IEnumerable<CommandError>>? OnRestoration { get; set; }

        public Func<IEnumerable<CommandError>>? OnValidation { get; set; }

        public bool RestoreCalled { get; private set; }

        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            this.ExecuteCalled = true;
            if (this.Error != null) throw this.Error;
            return Task.FromResult(new CommandResult(this.Number));
        }

        public override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            this.RestoreCalled = true;
            if (this.OnRestoration == null) return base.RestoreAsync(session);

            return Task.FromResult(this.OnRestoration());
        }

        public override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            this.ValidateCalled = true;
            if (this.OnValidation == null) return base.ValidateAsync(session);

            return Task.FromResult(this.OnValidation());
        }
    }
}
