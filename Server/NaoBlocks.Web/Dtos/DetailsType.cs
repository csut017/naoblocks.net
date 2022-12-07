namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Defines the type of details required.
    /// </summary>
    [Flags]
    public enum DetailsType
    {
        /// <summary>
        /// Exclude any details.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include the standard details.
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Include the parse result details.
        /// </summary>
        Parse = 2,
    }
}