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
        : CommandBase<Robot>
    {
        public string? FriendlyName { get; set; }

        public Password HashedPassword { get; set; } = Models.Password.Empty;

        public string? MachineName { get; set; }

        public string? Password { get; set; }

        public RobotType? RobotType { get; set; }

        public string? Type { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.Error($"Machine name is required for a robot"));
            }

            if (string.IsNullOrWhiteSpace(this.FriendlyName))
            {
                this.FriendlyName = this.MachineName;
            }

            if (string.IsNullOrWhiteSpace(this.Type))
            {
                errors.Add(this.Error($"Type is required for a robot"));
            }

            if (!errors.Any())
            {
                if (await session.Query<Robot>().AnyAsync(s => s.MachineName == this.MachineName).ConfigureAwait(false))
                {
                    errors.Add(this.Error($"Robot with name {this.MachineName} already exists"));
                }

                this.RobotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.Type)
                    .ConfigureAwait(false);
                if (this.RobotType == null)
                {
                    errors.Add(this.Error($"Unknown robot type {this.Type}"));
                }
            }

            this.HashedPassword = Models.Password.New(this.Password ?? string.Empty);
            this.Password = null;

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var robot = new Robot
            {
                MachineName = this.MachineName ?? "<Unknown>",
                FriendlyName = this.FriendlyName ?? "<Unknown>",
                Password = this.HashedPassword,
                IsInitialised = true,
                RobotTypeId = this.RobotType?.Id ?? string.Empty,
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(robot).ConfigureAwait(false);
            return this.Result(robot);
        }
    }
}