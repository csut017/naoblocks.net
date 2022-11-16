﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Common
{
    public class ClientMessage
    {
        public long? ConversationId { get; set; }

        public ClientMessageType Type { get; set; } = ClientMessageType.Unknown;

        public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();

        public ClientMessage()
        {
        }

        public ClientMessage(ClientMessageType type, object? values = null)
        {
            this.Type = type;
            if (values == null) return;
            foreach (var prop in values.GetType().GetProperties())
            {
                var value = prop.GetValue(values);
                if (value != null) this.Values[prop.Name] = value.ToString();
            }
        }

        public static ClientMessage FromArray(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<ClientMessage>(json);
        }

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