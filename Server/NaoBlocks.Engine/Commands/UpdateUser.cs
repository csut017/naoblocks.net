using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to update a user.
    /// </summary>
    public class UpdateUser
        : UserCommandBase
    {
        private User? person;

        /// <summary>
        /// Gets or sets the user's current name.
        /// </summary>
        public string? CurrentName { get; set; }

        /// <summary>
        /// Gets or sets the user's hashed password.
        /// </summary>
        public Password? HashedPassword { get; set; }

        /// <summary>
        /// Gets or sets the user's updated name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's plaintext password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Gets or sets the user's settings.
        /// </summary>
        public UserSettings? Settings { get; set; }

        /// <summary>
        /// Gets or sets the user's age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the user's gender.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Attempts to retrieve the existing user and validates the changes.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.CurrentName))
            {
                errors.Add(this.GenerateError($"Current name is required"));
            }

            if (!errors.Any())
            {
                this.person = await this.ValidateAndRetrieveUser(session, this.CurrentName, this.Role, errors).ConfigureAwait(false);
            }

            if (this.Password != null)
            {
                this.HashedPassword = Data.Password.New(this.Password);
                this.Password = null;
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the user in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            this.ValidateExecutionState(this.person);
            if (!string.IsNullOrWhiteSpace(this.Name) && (this.Name != this.person!.Name)) this.person.Name = this.Name.Trim();
            if (this.HashedPassword != null) this.person!.Password = this.HashedPassword;
            if (this.Settings != null) this.person!.Settings = this.Settings;

            if (this.person!.Role == UserRole.Student)
            {
                this.person.StudentDetails ??= new StudentDetails();
                if (this.Age != null) this.person.StudentDetails.Age = this.Age;
                if (this.Gender != null) this.person.StudentDetails.Gender = this.Gender;
            }

            return Task.FromResult(CommandResult.New(this.Number, this.person));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.CurrentName, this.Role, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}
