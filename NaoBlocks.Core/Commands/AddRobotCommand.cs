using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class AddRobotCommand
        : OutputCommandBase<Robot>
    {
        public string? FriendlyName { get; set; }

        public string? MachineName { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add($"Machine name is required for a robot");
            }

            if (string.IsNullOrWhiteSpace(this.FriendlyName))
            {
                this.FriendlyName = this.MachineName;
            }

            if (!errors.Any() && await session.Query<Robot>().AnyAsync(s => s.MachineName == this.MachineName).ConfigureAwait(false))
            {
                errors.Add($"Robot with name {this.MachineName} already exists");
            }

            return errors.AsEnumerable();
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var robot = new Robot
            {
                MachineName = this.MachineName ?? "<Unknown>",
                FriendlyName = this.FriendlyName ?? "<Unknown>",
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(robot).ConfigureAwait(false);
            this.Output = robot;
        }
    }
}