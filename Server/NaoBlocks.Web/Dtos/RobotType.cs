using NaoBlocks.Engine.Data;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot type.
    /// </summary>
    public class RobotType
    {
        /// <summary>
        /// Gets or sets whether this robot type allows direct logging.
        /// </summary>
        public bool? AllowDirectLogging { get; set; }

        /// <summary>
        /// Gets the allowed custom values.
        /// </summary>
        public IList<NamedValue>? CustomValues { get; private set; }

        /// <summary>
        /// Gets or sets whether this type has a toolbox.
        /// </summary>
        public bool? HasToolbox { get; set; }

        /// <summary>
        /// Gets or sets whether this type is the default robot type.
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the toolboxes.
        /// </summary>
        public IList<Toolbox>? Toolboxes { get; private set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">Whether to include the details or not.</param>
        /// <returns>A new <see cref="RobotType"/> instance containing the required properties.</returns>
        public static RobotType FromModel(Data.RobotType value, bool includeDetails = false)
        {
            var output = new RobotType
            {
                Name = value.Name,
                IsDefault = value.IsDefault,
                AllowDirectLogging = value.AllowDirectLogging,
                HasToolbox = value.Toolboxes.Any()
            };

            if (includeDetails)
            {
                output.Toolboxes = value.Toolboxes.Select(t => Toolbox.FromModel(t)).ToList();
                output.CustomValues = value.CustomValues.ToList();
            }

            return output;
        }
    }
}