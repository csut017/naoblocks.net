using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteRobotTypeCommand
        : CommandBase
    {
        private RobotType? robotType;

        public string? Name { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.Error($"Machine name is required for robot"));
            }

            if (!errors.Any())
            {
                this.robotType = await session.Query<RobotType>()
                                            .FirstOrDefaultAsync(u => u.Name == this.Name)
                                            .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.Error($"Robot type {this.Name} does not exist"));
                }
                else
                {
                    var hasRobots = await session.Query<Robot>()
                        .AnyAsync(r => r.RobotTypeId == this.robotType.Id)
                        .ConfigureAwait(false);

                    if (hasRobots)
                    {
                        errors.Add(this.Error($"Robot type {this.Name} has robots instances"));
                    }
                }
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.robotType);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}