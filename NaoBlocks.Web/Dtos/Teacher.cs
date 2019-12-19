using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Teacher
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public static Teacher FromModel(Data.User value)
        {
            return value == null ? null : new Teacher
            {
                Name = value.Name
            };
        }
    }
}