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
        public Password? HashedPassword { get; set; }
        public string? MachineName { get; set; }

        public string? Password { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.robot = await session.Query<Robot>()
                                        .FirstOrDefaultAsync(u => u.MachineName == this.CurrentMachineName)
                                        .ConfigureAwait(false);
            if (this.robot == null) errors.Add(this.Error($"Robot {this.CurrentMachineName} does not exist"));

            if (this.Password != null)
            {
                this.HashedPassword = Models.Password.New(this.Password);
                this.Password = null;
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.robot == null) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.MachineName) && (this.MachineName != this.robot.MachineName)) this.robot.MachineName = this.MachineName;
            if (!string.IsNullOrEmpty(this.FriendlyName) && (this.FriendlyName != this.robot.FriendlyName)) this.robot.FriendlyName = this.FriendlyName;
            if (this.HashedPassword != null)
            {
                this.robot.Password = this.HashedPassword;
                this.robot.IsInitialised = true;
            }

            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}