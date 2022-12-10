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
        public IList<Data.NamedValue>? CustomValues { get; private set; }

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
        /// Gets or sets the parse results.
        /// </summary>
        public ParseResults? Parse { get; set; }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        public IList<Data.LoggingTemplate>? Templates { get; private set; }

        /// <summary>
        /// Gets the toolboxes.
        /// </summary>
        public IList<Toolbox>? Toolboxes { get; private set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="RobotType"/> instance containing the required properties.</returns>
        public static RobotType FromModel(Data.RobotType value, DetailsType includeDetails = DetailsType.None)
        {
            var output = new RobotType
            {
                Name = value.Name,
                IsDefault = value.IsDefault,
                AllowDirectLogging = value.AllowDirectLogging,
                HasToolbox = value.Toolboxes.Any()
            };

            if (includeDetails.HasFlag(DetailsType.Standard))
            {
                output.Toolboxes = value.Toolboxes.Select(t => Toolbox.FromModel(t, includeDetails)).ToList();
                output.CustomValues = value.CustomValues.ToList();
                output.Templates = value.LoggingTemplates.ToList();
            }

            return output;
        }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="RobotType"/> instance containing the required properties.</returns>
        public static RobotType FromModel(Data.RobotTypeImport value, DetailsType includeDetails = DetailsType.None)
        {
            if (value.RobotType == null) throw new ArgumentNullException(nameof(value), "RobotType cannot be null");
            var output = new RobotType
            {
                Name = value.RobotType.Name,
                IsDefault = value.RobotType.IsDefault,
                AllowDirectLogging = value.RobotType.AllowDirectLogging,
                HasToolbox = value.RobotType.Toolboxes.Any()
            };

            if (includeDetails.HasFlag(DetailsType.Standard))
            {
                output.Toolboxes = value.RobotType.Toolboxes.Select(t => Toolbox.FromModel(t, includeDetails)).ToList();
                output.CustomValues = value.RobotType.CustomValues.ToList();
                output.Templates = value.RobotType.LoggingTemplates.ToList();
            }

            if (includeDetails.HasFlag(DetailsType.Parse))
            {
                output.Parse = new ParseResults { Message = value.RobotType.Message ?? string.Empty };
                output.Parse.Details["duplicate"] = value.IsDuplicate;
            }

            return output;
        }
    }
}