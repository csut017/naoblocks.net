using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Student : User
    {
        public Data.UserSettings? Settings { get; set; }

        public static Student FromModel(Data.User value, bool includeDetails = false)
        {
            var student = new Student
            {
                Name = value.Name,
                WhenAdded = value.WhenAdded
            };
            if (includeDetails)
            {
                student.Settings = value.Settings;
            }
            return student;
        }
    }
}