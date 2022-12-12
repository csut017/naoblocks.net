namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a robot type import definition.
    /// </summary>
    public class RobotTypeImport
        : ItemImport<RobotType>
    {
        /// <summary>
        /// Gets or sets the associated robots.
        /// </summary>
        public List<ItemImport<Robot>>? Robots { get; set; }
    }
}