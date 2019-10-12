using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public class AppSettings
    {
        public string JwtSecret { get; set; }

        public string[] DatabaseUrls { get; set; }

        public string DatabaseName { get; set; }
    }
}
