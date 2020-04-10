using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class RobotType
    {
        public string Name { get; set; } = string.Empty;

        public static RobotType? FromModel(Data.RobotType value)
        {
            return value == null ? null : new RobotType
            {
                Name = value.Name
            };
        }
    }
}