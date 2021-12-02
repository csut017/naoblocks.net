namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A notification from the client about something that has occurred.
    /// </summary>
    public class NotificationAlert
    {
        /// <summary>
        /// Gets or sets the identifier of the alert.
        /// </summary>
        public int Id { get; set; } = -1;

        /// <summary>
        /// Gets or sets the alert message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alert severity.
        /// </summary>
        public string Severity { get; set; } = "info";

        /// <summary>
        /// Gets or sets when the alert was added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.MinValue;
    }
}