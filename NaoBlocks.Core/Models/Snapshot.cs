using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class Snapshot
    {
        public string Source { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        [JsonIgnore]
        public User? User { get; set; }

        public string UserId { get; set; } = string.Empty;

        public IList<NamedValue> Values { get; } = new List<NamedValue>();

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;
    }
}
