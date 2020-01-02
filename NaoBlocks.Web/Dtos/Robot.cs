using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Robot
    {
        public string FriendlyName { get; set; } = string.Empty;

        public bool IsInitialised { get; set; }

        public string MachineName { get; set; } = string.Empty;

        public string? Password { get; set; }

        public static Robot? FromModel(Data.Robot value)
        {
            return value == null ? null : new Robot
            {
                FriendlyName = value.FriendlyName,
                IsInitialised = value.IsInitialised,
                MachineName = value.MachineName
            };
        }
    }
}