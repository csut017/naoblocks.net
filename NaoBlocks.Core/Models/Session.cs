using System;

namespace NaoBlocks.Core.Models
{
    public class Session
    {
        public string? Id { get; set; }

        public bool IsRobot { get; set; }

        public UserRole? Role { get; set; }

        public string? UserId { get; set; }

        public DateTime WhenAdded { get; set; }

        public DateTime WhenExpires { get; set; }
    }
}