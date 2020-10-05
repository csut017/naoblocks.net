using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class EditorSettings
    {
        public bool CanConfigure { get; set; } = true;

        public Data.UserSettings? User { get; set; }

        public string? Toolbox { get; set; }

        public bool IsSystemInitialised { get; set; }
    }
}
