using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A DTO for a synchronization source.
    /// </summary>
    public class SynchronizationSource
    {
        /// <summary>
        /// Gets or sets the source address.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets when the source was added.
        /// </summary>
        public DateTime? WhenAdded { get; set; }

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeDetails">The types of details to include.</param>
        /// <returns>A new <see cref="SynchronizationSource"/> instance containing the required properties.</returns>
        public static SynchronizationSource FromModel(Data.SynchronizationSource value, DetailsType includeDetails = DetailsType.None)
        {
            var item = new SynchronizationSource
            {
                Name = value.Name,
                Address = value.Address,
                WhenAdded = value.WhenAdded
            };

            return item;
        }
    }
}