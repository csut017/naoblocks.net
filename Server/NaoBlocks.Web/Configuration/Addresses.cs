namespace NaoBlocks.Web.Configuration
{
    /// <summary>
    /// The configuration information for the addresses.
    /// </summary>
    public class Addresses
    {
        /// <summary>
        /// Gets or sets the HTTP port
        /// </summary>
        public int HttpPort { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the HTTPS port.
        /// </summary>
        public int HttpsPort { get; set; } = 5001;

        /// <summary>
        /// Gets or sets whether to listen on localhost (127.0.0.1)
        /// </summary>
        public bool ListenOnLocalHost { get; set; }

        /// <summary>
        /// Gets or sets whether to use HTTP or not.
        /// </summary>
        public bool UseHttp { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use HTTPS or not.
        /// </summary>
        public bool UseHttps { get; set; } = true;
    }
}