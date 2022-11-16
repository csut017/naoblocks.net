using Newtonsoft.Json;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A snapshot of the code at a point in time.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        /// Gets or sets the source of the snapshot.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user this snapshot is for.
        /// </summary>
        [JsonIgnore]
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the id of the associated user.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets any associated values.
        /// </summary>
        public IList<NamedValue> Values { get; } = new List<NamedValue>();

        /// <summary>
        /// Gets or sets the date and time this snapshot ws added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;
    }
}
