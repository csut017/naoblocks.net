using System;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Student : User
    {
        public int? Age { get; set; }

        public string Gender { get; set; } = "Unknown";

        public static Student FromModel(Data.User value, bool includeDetails = false)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var student = new Student
            {
                Name = value.Name,
                WhenAdded = value.WhenAdded,
                Age = value.StudentDetails?.Age,
                Gender = value.StudentDetails?.Gender ?? "Unknown"
            };
            if (includeDetails)
            {
                student.Settings = value.Settings;
            }
            return student;
        }
    }
}