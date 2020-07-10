using System.Collections.Generic;

namespace NaoBlocks.Web.Communications
{
    public class RobotStatus
    {
        public int? LastProgramId { get; set; }

        public IList<string> SourceIds { get; } = new List<string>();
    }
}
