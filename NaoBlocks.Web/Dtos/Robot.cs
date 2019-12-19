using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Robot
    {
        public string FriendlyName { get; set; }

        public string MachineName { get; set; }

        public static Robot FromModel(Data.Robot value)
        {
            return value == null ? null : new Robot
            {
                FriendlyName = value.FriendlyName,
                MachineName = value.MachineName
            };
        }
    }
}