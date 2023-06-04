namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A token for a specific machine.
    /// </summary>
    public class MachineToken
    {
        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        public string? Machine { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string? Token { get; set; }
    }
}