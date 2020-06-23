using System.IO;
using System.Net;

namespace NaoBlocks.Web.Helpers
{
    public static partial class RobotTypeFilePackage
    {
        public struct FileDetails
        {
            public HttpStatusCode StatusCode { get; set; }

            public Stream? DataStream { get; set; }
        }
    }
}
