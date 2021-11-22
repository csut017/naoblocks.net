using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace NaoBlocks.Common
{
    /// <summary>
    /// A messages that can be passed via websockets.
    /// </summary>
    public class ClientMessage
    {
        /// <summary>
        /// Gets or sets the identifier for the conversation this message is part of.
        /// </summary>
        public long? ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public ClientMessageType Type { get; set; } = ClientMessageType.Unknown;

        /// <summary>
        /// Gets the associated values for this message.
        /// </summary>
        public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Initialises a new empty <see cref="ClientMessage"/> instance.
        /// </summary>
        public ClientMessage()
        {
        }

        /// <summary>
        /// Initialises a new <see cref="ClientMessage"/> instance from existing data.
        /// </summary>
        /// <param name="type">The <see cref="ClientMessageType"/> of the message.</param>
        /// <param name="values">An object containing the initial values.</param>
        public ClientMessage(ClientMessageType type, object? values = null)
        {
            this.Type = type;
            if (values == null) return;
            foreach (var prop in values.GetType().GetProperties())
            {
                var value = prop.GetValue(values);
                this.Values[prop.Name] = value?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Generates a new <see cref="ClientMessage"/> from a message buffer.
        /// </summary>
        /// <param name="data">The message buffer.</param>
        /// <returns>A new <see cref="ClientMessage"/> instance.</returns>
        /// <remarks>
        /// The buffer needs to contain UTF-8 encoded JSON.
        /// </remarks>
        public static ClientMessage FromArray(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<ClientMessage>(json) ?? new ClientMessage();
        }

        /// <summary>
        /// Generates a shallow clone of the <see cref="ClientMessage"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ClientMessage"/> containing the cloned data.</returns>
        public ClientMessage Clone()
        {
            var message = new ClientMessage
            {
                ConversationId = this.ConversationId,
                Type = this.Type
            };
            foreach (var (key, value) in this.Values)
            {
                message.Values.Add(key, value);
            }
            return message;
        }

        /// <summary>
        /// Converts this <see cref="ClientMessage"/> instance to a buffer.
        /// </summary>
        /// <returns>A <see cref="byte"/> array containing UTF-8 encoded JSON.</returns>
        public byte[] ToArray()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, settings);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
