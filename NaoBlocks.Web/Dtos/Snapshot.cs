using System;
using System.Collections.Generic;
using System.Linq;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Snapshot
    {
        public string Source { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string? User { get; set; }

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        public IList<Data.NamedValue>? Values { get; set; }

        public static Snapshot? FromModel(Data.Snapshot? value)
        {
            if (value == null) return null;
            var snapshot = new Snapshot
            {
                Source = value.Source,
                State = value.State,
                WhenAdded = value.WhenAdded,
                User = value.User?.Name
            };

            if (value.Values.Any())
            {
                snapshot.Values = new List<Data.NamedValue>(value.Values);
            }

            return snapshot;
        }
    }
}