using NaoBlocks.Common;
using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot log line.
    /// </summary>
    public class RobotLogLine
    {
        /// <summary>
        /// Gets or sets the line description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source message type.
        /// </summary>
        public ClientMessageType SourceMessageType { get; set; }

        /// <summary>
        /// Gets or sets the line values.
        /// </summary>
        public IDictionary<string, string>? Values { get; private set; }

        /// <summary>
        /// Gets or sets when the line was added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="RobotLogLine"/> instance containing the required properties.</returns>
        public static RobotLogLine FromModel(Data.RobotLogLine value)
        {
            var robotLog = new RobotLogLine
            {
                Description = value.Description,
                WhenAdded = value.WhenAdded,
                SourceMessageType = value.SourceMessageType
            };
            if (value.Values.Any())
            {
                robotLog.Values = value.Values
                    .ToDictionary(v => v.Name, v => v.Value ?? string.Empty);
            }

            return robotLog;
        }
    }
}
