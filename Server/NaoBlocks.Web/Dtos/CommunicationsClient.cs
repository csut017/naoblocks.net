using NaoBlocks.Web.Communications;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a communications client.
    /// </summary>
    public class CommunicationsClient
    {
        /// <summary>
        /// Gets or sets the client's status.
        /// </summary>
        public ClientStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the client's type.
        /// </summary>
        public ClientConnectionType Type { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets whether the client connection is currently closing.
        /// </summary>
        public bool IsClosing { get; private set; }

        /// <summary>
        /// Gets or sets the associated robot details.
        /// </summary>
        public Robot? Robot { get; set; }

        /// <summary>
        /// Gets or sets the associated user details.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Converts a connection entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="CommunicationsClient"/> instance containing the required properties.</returns>
        public static CommunicationsClient FromModel(ClientConnection value)
        {
            return new CommunicationsClient
            {
                Id = value.Id,
                IsClosing = value.IsClosing,
                Status = value.Type == ClientConnectionType.Robot ? value.Status : null,
                Type = value.Type,
                Robot = value.Robot == null ? null : Robot.FromModel(value.Robot),
                User = value.User == null ? null : User.FromModel(value.User)
            };
        }
    }
}
