using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Web.Communications
{
    public class ClientMessage
    {
        public long ClientId { get; set; }

        public ClientMessageType Type { get; set; } = ClientMessageType.Unknown;

        public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();

        public static ClientMessage FromArray(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<ClientMessage>(json);
        }

        public byte[] ToArray()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}