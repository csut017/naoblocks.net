using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteRobotCommand
        : CommandBase
    {
        private Robot? robot;

        public string? MachineName { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add($"Machine name is required for robot");
            }

            if (!errors.Any())
            {
                this.robot = await session.Query<Robot>()
                                            .FirstOrDefaultAsync(u => u.MachineName == this.MachineName)
                                            .ConfigureAwait(false);
                if (this.robot == null) errors.Add($"Robot {this.MachineName} does not exist");
            }

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.robot);
            return Task.CompletedTask;
        }
    }
}