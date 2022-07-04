using NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A data transfer object for blocks and block sets for a robot type.
    /// </summary>
    public class RobotBlockDefinitions
    {
        /// <summary>
        /// The available blocks.
        /// </summary>
        public IEnumerable<BlockDefinition>? Blocks { get; set; }

        /// <summary>
        /// The available block sets.
        /// </summary>
        public IEnumerable<NamedValue>? BlockSets { get; set; }
    }
}