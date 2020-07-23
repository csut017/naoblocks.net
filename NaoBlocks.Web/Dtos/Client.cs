using NaoBlocks.Core.Models;
using NaoBlocks.Web.Communications;

namespace NaoBlocks.Web.Dtos
{
    public class Client
    {
        public ClientStatus? Status { get; set; }

        public ClientConnectionType Type { get; set; }

        public long Id { get; set; }

        public bool IsClosing { get; private set; }

        public Robot? Robot { get; set; }

        public User? User { get; set; }

        public static Client? FromModel(ClientConnection? value)
        {
            if (value == null) return null;

            return new Client
            {
                Id = value.Id,
                IsClosing = value.IsClosing,
                Status = value.Type == ClientConnectionType.Robot ? value.Status : null,
                Type = value.Type,
                Robot = value.Robot == null ? null : Robot.FromModel(value.Robot),
                User = value.User == null ? null : User.FromModel(value.User)
            };
        }
    }
}
