﻿using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class CommandManager : ICommandManager
    {
        private readonly IAsyncDocumentSession session;
        private readonly IDocumentStore store;

        public CommandManager(IDocumentStore store, IAsyncDocumentSession session)
        {
            this.store = store;
            this.session = session;
        }

        public async Task<CommandResult> ApplyAsync(CommandBase? command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            var result = await command.ApplyAsync(this.session).ConfigureAwait(false);
            var log = new CommandLog
            {
                WhenApplied = command.WhenExecuted,
                Command = command,
                Result = result,
                Type = command.GetType().Name
            };
            if (log.Type.EndsWith("Command", StringComparison.InvariantCulture)) log.Type = log.Type.Substring(0, log.Type.Length - 7);

            // Always store the command log - use a seperate session to ensure it is saved
            using (var logSession = this.store.OpenAsyncSession())
            {
                await logSession.StoreAsync(log).ConfigureAwait(false);
                await logSession.SaveChangesAsync().ConfigureAwait(false);
            }

            return result;
        }

        public async Task CommitAsync()
        {
            await this.session.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<IEnumerable<string>> ValidateAsync(CommandBase? command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return command.ValidateAsync(this.session);
        }
    }
}