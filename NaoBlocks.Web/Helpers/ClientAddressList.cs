using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public static class ClientAddressList
    {
        private static readonly List<string> addresses = new List<string>();

        public static void Add(params string[] address)
        {
            addresses.AddRange(address);
        }

        public static ReadOnlyCollection<string> Get()
        {
            return new ReadOnlyCollection<string>(addresses);
        }
    }
}
