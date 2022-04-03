using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot type.
    /// </summary>
    public class RobotType
    {
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
        /// Gets the categories in the toolbox.
        /// </summary>
        public IList<Data.ToolboxCategory>? Toolbox { get; private set; }

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
                HasToolbox = value.Toolbox.Any()
            };

            if (includeDetails)
            {
                output.Toolbox = value.Toolbox;
            }

            return output;
        }
    }
}