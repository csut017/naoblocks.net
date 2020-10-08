using System;

namespace NaoBlocks.Core.Models
{
    public class User
    {
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public long NextProgramNumber { get; set; }

        public Password Password { get; set; } = Password.Empty;

        public UserRole Role { get; set; }

        public UserSettings Settings { get; set; } = new UserSettings();

        public DateTime WhenAdded { get; set; }

        public StudentDetails? StudentDetails { get; set; }
    }
}