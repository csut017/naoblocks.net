using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Student
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public static Student FromModel(Data.User value)
        {
            return value == null ? null : new Student
            {
                Name = value.Name
            };
        }
    }
}