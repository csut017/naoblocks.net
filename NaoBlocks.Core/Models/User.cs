using System;

namespace NaoBlocks.Core.Models
{
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Password Password { get; set; }

        public UserRole Role { get; set; }

        public DateTime WhenAdded { get; set; }
    }
}