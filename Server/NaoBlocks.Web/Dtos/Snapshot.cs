using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a snapshot.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        /// Gets or sets the source of the snapshot.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state of the snapshot.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the associated user.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets when the snapshot was added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the associated values.
        /// </summary>
        public IList<Data.NamedValue>? Values { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="Snapshot"/> instance containing the required properties.</returns>
        public static Snapshot FromModel(Data.Snapshot value)
        {
            var snapshot = new Snapshot
            {
                Source = value.Source,
                State = value.State,
                WhenAdded = value.WhenAdded,
                User = value.User?.Name
            };

            if (value.Values.Any())
            {
                snapshot.Values = new List<Data.NamedValue>(value.Values);
            }

            return snapshot;
        }
    }
}
