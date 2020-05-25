using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Core.Models
{
    public class Conversation
    {
        public long ConversationId { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; }
    }
}
