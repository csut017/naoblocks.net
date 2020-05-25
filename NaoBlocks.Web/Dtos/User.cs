using System;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class User
    {
        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        public DateTime? WhenAdded { get; set; }

        public static User FromModel(Data.User value)
        {
            var student = new Student
            {
                Name = value.Name,
                Role = value.Role.ToString(),
                WhenAdded = value.WhenAdded
            };
            return student;
        }
    }
}