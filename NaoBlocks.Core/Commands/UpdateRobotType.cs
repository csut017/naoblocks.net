using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class UpdateRobotType
        : CommandBase<RobotType>
    {
        private RobotType? robotType;

        public string? CurrentName { get; set; }

        public bool? IsDefault { get; set; }

        public string? Name { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.robotType = await session.Query<RobotType>()
                                        .FirstOrDefaultAsync(u => u.Name == this.CurrentName)
                                        .ConfigureAwait(false);
            if (this.robotType == null) errors.Add(this.GenerateError($"Robot type {this.CurrentName} does not exist"));

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.robotType == null) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.Name) && (this.Name != this.robotType.Name)) this.robotType.Name = this.Name;
            if (this.IsDefault.HasValue) this.robotType.IsDefault = this.IsDefault.Value;

            return Task.FromResult(CommandResult.New(this.Number, this.robotType));
        }
    }
}