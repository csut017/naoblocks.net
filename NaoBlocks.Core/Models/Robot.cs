using System;

namespace NaoBlocks.Core.Models
{
    public class Robot
    {
        public string FriendlyName { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        public bool IsInitialised { get; set; }

        public string MachineName { get; set; } = string.Empty;

        public Password Password { get; set; } = Password.Empty;

        public DateTime WhenAdded { get; set; }
    }
}