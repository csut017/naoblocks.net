using NaoBlocks.Core.Models;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class StoreSettings
        : CommandBase<UserSettings>
    {
        public UserSettings? Settings { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        public string? UserId { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();

            if (this.Settings == null)
            {
                errors.Add(this.Error($"Settings are required"));
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Settings.RobotType))
                {
                    var robotType = await session.Query<RobotType>()
                        .FirstOrDefaultAsync(rt => rt.Name == this.Settings.RobotType)
                        .ConfigureAwait(false);
                    if (robotType == null)
                    {
                        errors.Add(this.Error($"Unknown robot type {this.Settings.RobotType}"));
                    }
                    else
                    {
                        this.Settings.RobotTypeId = robotType.Id;
                    }
                }
            }

            if (this.User == null)
            {
                if (string.IsNullOrWhiteSpace(this.UserId))
                {
                    errors.Add(this.Error($"UserID is required for storing settings"));
                }

                if (!errors.Any())
                {
                    this.User = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == this.UserId).ConfigureAwait(false);
                    if (this.User == null)
                    {
                        errors.Add(this.Error($"User does not exist"));
                    }
                }
            }
            else
            {
                this.UserId = this.User.Id;
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.User == null) throw new InvalidCallOrderException();
            var settingsToStore = this.Settings ?? new UserSettings();
            User.Settings = settingsToStore;
            CommandResult result = this.Result(settingsToStore);
            return Task.FromResult(result);
        }
    }
}