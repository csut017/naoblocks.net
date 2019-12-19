using System;

namespace NaoBlocks.Core.Models
{
    public class Robot
    {
        public string FriendlyName { get; set; }

        public string Id { get; set; }

        public string MachineName { get; set; }

        public DateTime WhenAdded { get; set; }
    }
}