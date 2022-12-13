using NaoBlocks.Engine.Data;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Gets or sets the robot's friendly (human-readable) name.
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the robot is initialised.
        /// </summary>
        public bool IsInitialised { get; set; }

        /// <summary>
        /// Gets or sets the robot's machine name.
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parse results.
        /// </summary>
        public ParseResults? Parse { get; set; }

        /// <summary>
        /// Gets or sets the robot's password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the robot type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets the allowed custom values.
        /// </summary>
        public IList<NamedValue>? Values { get; private set; }

        /// <summary>
        /// Gets or sets when the robot was added.
        /// </summary>
        public DateTime? WhenAdded { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="Robot"/> instance containing the required properties.</returns>
        public static Robot FromModel(Data.Robot value, DetailsType includeDetails = DetailsType.None)
        {
            var type = value.Type?.Name ?? value.RobotTypeId;
            var output = new Robot
            {
                FriendlyName = value.FriendlyName,
                IsInitialised = value.IsInitialised,
                MachineName = value.MachineName,
                Type = string.IsNullOrEmpty(type) ? null : type,
                WhenAdded = value.WhenAdded,
            };

            if (includeDetails.HasFlag(DetailsType.Standard))
            {
                output.Values = value.CustomValues.ToList();
            }

            return output;
        }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="Robot"/> instance containing the required properties.</returns>
        public static Robot FromModel(Data.ItemImport<Data.Robot> value, DetailsType includeDetails = DetailsType.None)
        {
            if (value.Item == null) throw new ArgumentNullException(nameof(value), "Item in value has not been set");
            var output = FromModel(value.Item, includeDetails);
            if (includeDetails.HasFlag(DetailsType.Parse) && (value != null))
            {
                output.Parse = ParseResults.FromModel(value);
                output.Password = value.Item.PlainPassword ?? string.Empty;
            }

            return output;
        }
    }
}