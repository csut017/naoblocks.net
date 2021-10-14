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
    public class AddBlockSet
        : CommandBase<BlockSet>
    {
        private RobotType? robotType;

        public string? Name { get; set; }

        public string? RobotType { get; set; }

        public string? Categories { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for a block set"));
            }

            if (string.IsNullOrWhiteSpace(this.Categories))
            {
                errors.Add(this.GenerateError($"Categories is required for a block set"));
            }

            if (string.IsNullOrWhiteSpace(this.RobotType))
            {
                errors.Add(this.GenerateError($"Robot Type is required for a block set"));
            }

            if (!errors.Any())
            {
                this.robotType = await session.Query<RobotType>().FirstOrDefaultAsync(s => s.Name == this.RobotType).ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Robot type with name {this.Name} does not exist"));
                }
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.robotType == null) throw new InvalidOperationException("ValidateAsync must be called first");

            var set = new BlockSet
            {
                Name= this.Name?? "<Unknown>",
                BlockCategories = this.Categories ?? string.Empty
            };

            this.robotType.BlockSets.Add(set);
            return Task.FromResult((CommandResult)this.Result(set));
        }
    }
}