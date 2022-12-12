using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets a message associated with the user.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the user settings.
        /// </summary>
        public Data.UserSettings? Settings { get; set; }

        /// <summary>
        /// Gets or sets the login token for the user.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Gets or sets when the user was added.
        /// </summary>
        public DateTime? WhenAdded { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="User"/> instance containing the required properties.</returns>
        public static User FromModel(Data.User value, DetailsType includeDetails = DetailsType.None)
        {
            var user = new User
            {
                Name = value.Name,
                Role = value.Role.ToString(),
                Password = value.PlainPassword,
                WhenAdded = value.WhenAdded
            };

            if (includeDetails.HasFlag(DetailsType.Standard))
            {
                user.Settings = value.Settings;
            }

            return user;
        }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="User"/> instance containing the required properties.</returns>
        public static User FromModel(Data.ItemImport<Data.User> value, DetailsType includeDetails = DetailsType.None)
        {
            if (value.Item == null) throw new ArgumentNullException(nameof(value), "Item in value has not been set");
            var output = FromModel(value.Item, includeDetails);
            if (includeDetails.HasFlag(DetailsType.Parse))
            {
                output.Message = value.Message;
            }

            return output;
        }
    }
}