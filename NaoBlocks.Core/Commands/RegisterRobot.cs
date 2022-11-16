﻿using NaoBlocks.Common;
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
    public class RegisterRobot
        : CommandBase<Robot>
    {
        public string? MachineName { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required for a robot"));
            }

            if (!errors.Any() && await session.Query<Robot>().AnyAsync(s => s.MachineName == this.MachineName).ConfigureAwait(false))
            {
                errors.Add(this.GenerateError($"Robot with name {this.MachineName} already exists"));
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var robot = new Robot
            {
                MachineName = this.MachineName ?? "<Unknown>",
                FriendlyName = this.MachineName ?? "<Unknown>",
                IsInitialised = false,
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(robot).ConfigureAwait(false);
            return this.Result(robot);
        }
    }
}