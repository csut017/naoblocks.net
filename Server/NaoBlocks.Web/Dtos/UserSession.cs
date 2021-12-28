namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A data transfer object for user session details.
    /// </summary>
    public class UserSession : User
    {
        /// <summary>
        /// Gets or sets the time remaining for the session.
        /// </summary>
        public int? TimeRemaining { get; set; }
    }
}