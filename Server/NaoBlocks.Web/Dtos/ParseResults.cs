namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Includes details on the parsing results.
    /// </summary>
    public class ParseResults
    {
        /// <summary>
        /// Gets the details of the parse.
        /// </summary>
        public IDictionary<string, object> Details { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}