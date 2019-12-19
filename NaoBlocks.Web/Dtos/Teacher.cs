using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Teacher : User
    {
        public Teacher()
        {
            this.Role = Data.UserRole.Teacher;
        }

        public static Teacher? FromModel(Data.User? value)
        {
            return value == null ? null : new Teacher
            {
                Name = value.Name
            };
        }
    }
}