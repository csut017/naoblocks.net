using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class User
    {
        public string? Name { get; set; }

        public string? Password { get; set; }

        public Data.UserRole Role { get; set; }
    }
}