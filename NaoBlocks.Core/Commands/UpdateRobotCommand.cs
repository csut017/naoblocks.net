using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class UpdateRobotCommand
        : CommandBase
    {
        private Robot? robot;

        public string? CurrentMachineName { get; set; }

        public string? FriendlyName { get; set; }
        public string? MachineName { get; set; }

        public async override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<string>();
            this.robot = await session.Query<Robot>()
                                        .FirstOrDefaultAsync(u => u.MachineName == this.CurrentMachineName)
                                        .ConfigureAwait(false);
            if (this.robot == null) errors.Add($"Robot {this.CurrentMachineName} does not exist");

            return errors.AsEnumerable();
        }

        protected override Task DoApplyAsync(IAsyncDocumentSession? session, CommandResult? result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.robot == null) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.MachineName) && (this.MachineName != this.robot.MachineName)) this.robot.MachineName = this.MachineName;
            if (!string.IsNullOrEmpty(this.FriendlyName) && (this.FriendlyName != this.robot.FriendlyName)) this.robot.FriendlyName = this.FriendlyName;
            return Task.CompletedTask;
        }
    }
}