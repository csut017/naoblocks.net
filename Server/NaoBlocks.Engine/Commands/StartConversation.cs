﻿using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for starting a conversation.
    /// </summary>
    public class StartConversation
        : UserCommandBase
    {
        private User? person;

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Validates the user via their name and password.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required"));
            }

            if (!errors.Any())
            {
                this.person = await this.ValidateAndRetrieveUser(session, this.Name, UserRole.User, errors)
                    .ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.person);

            var systemValues = await session.Query<SystemValues>().FirstOrDefaultAsync();
            if (systemValues == null)
            {
                systemValues = new SystemValues();
                await session.StoreAsync(systemValues);
            }
            var conversationId = ++systemValues.NextConversationId;
            var conversation = new Conversation
            {
                ConversationId = conversationId,
                UserId = this.person!.Id,
                UserName = this.person.Name
            };
            await session.StoreAsync(conversation);

            return CommandResult.New(this.Number, conversation);
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.Name, UserRole.User, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}