using System.Collections.Generic;

namespace NaoBlocks.Web.Dtos
{
    public class SystemStatus
    {
        public IList<RobotStatus> RobotsConnected { get; } = new List<RobotStatus>();

        public IList<UserStatus> UsersConnected { get; } = new List<UserStatus>();
    }
}