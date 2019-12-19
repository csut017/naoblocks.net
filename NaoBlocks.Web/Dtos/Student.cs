using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Student : User
    {
        public Student()
        {
            this.Role = Data.UserRole.Student;
        }

        public static Student? FromModel(Data.User? value)
        {
            return value == null ? null : new Student
            {
                Name = value.Name
            };
        }
    }
}