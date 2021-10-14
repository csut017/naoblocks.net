using NaoBlocks.Common;
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
    public class AddRobotType
        : CommandBase<RobotType>
    {
        public string? Name { get; set; }

        public bool IsDefault { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for a robot type"));
            }

            if (!errors.Any() && await session.Query<RobotType>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add(this.GenerateError($"Robot type with name {this.Name} already exists"));
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var robot = new RobotType
            {
                Name= this.Name?? "<Unknown>",
                IsDefault = this.IsDefault,
                WhenAdded = this.WhenExecuted
            };

            await session.StoreAsync(robot).ConfigureAwait(false);
            return this.Result(robot);
        }
    }
}