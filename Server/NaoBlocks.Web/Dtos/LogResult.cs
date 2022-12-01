﻿namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Contains the results of a logging operation.
    /// </summary>
    public class LogResult
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <remarks>
        /// This property will be null if there is no error.
        /// </remarks>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets the map to use.
        /// </summary>]
        public Dictionary<string, string>? Values { get; set; }
    }
}