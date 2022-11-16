using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a toolbox.
    /// </summary>
    public class Toolbox
    {
        /// <summary>
        /// Gets or sets the toolbox definition.
        /// </summary>
        public string? Definition { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default toolbox for the robot type.
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this toolbox uses events.
        /// </summary>
        public bool UseEvents { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">Whether to include the details or not.</param>
        /// <param name="format">
        /// The format to use for the definition. Options are "toolbox" for the toolbox format or "blockly" for use in a blockly editor.
        /// The default format is "toolbox".
        /// </param>
        /// <returns>A new <see cref="Toolbox"/> instance containing the required properties.</returns>
        public static Toolbox FromModel(Data.Toolbox value, bool includeDetails = false, string? format = null)
        {
            var output = new Toolbox
            {
                Name = value.Name,
                IsDefault = value.IsDefault,
                UseEvents = value.UseEvents
            };

            if (includeDetails)
            {
                if (!Enum.TryParse<Data.Toolbox.Format>(format, true, out var formatType))
                {
                    formatType = Data.Toolbox.Format.Toolbox;
                }

                output.Definition = value.ExportToXml(formatType);
            }

            return output;
        }
    }
}