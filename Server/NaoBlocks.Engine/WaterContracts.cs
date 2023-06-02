using Newtonsoft.Json.Serialization;

namespace NaoBlocks.Engine
{
    internal class DehydrationContractResolver
        : DefaultContractResolver
    {
        private readonly Dictionary<string, string> mappings = new();

        public DehydrationContractResolver()
        {
            mappings.Add("AllowDirectLogging", "adl");
            mappings.Add("Command", "c");
            mappings.Add("CurrentName", "cn");
            mappings.Add("ConversationType", "ct");
            mappings.Add("Conversation", "cv");
            mappings.Add("Description", "d");
            mappings.Add("FriendlyName", "fn");
            mappings.Add("Hash", "h");
            mappings.Add("HashedPassword", "hp");
            mappings.Add("Id", "i");
            mappings.Add("MachineName", "mn");
            mappings.Add("Name", "n");
            mappings.Add("RobotTypeName", "rtn");
            mappings.Add("Salt", "s");
            mappings.Add("SourceId", "si");
            mappings.Add("SourceMessageType", "smt");
            mappings.Add("SourceName", "sn");
            mappings.Add("SourceType", "st");
            mappings.Add("Type", "t");
            mappings.Add("Value", "v");
            mappings.Add("Values", "vs");
            mappings.Add("WhenApplied", "wa");
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return mappings.TryGetValue(propertyName, out var value)
                ? value
                : base.ResolvePropertyName(propertyName);
        }
    }

    internal class HydrationContractResolver
        : DefaultContractResolver
    {
        private readonly Dictionary<string, string> mappings = new();

        public HydrationContractResolver()
        {
            mappings.Add("adl", "AllowDirectLogging");
            mappings.Add("c", "Command");
            mappings.Add("cn", "CurrentName");
            mappings.Add("ct", "ConversationType");
            mappings.Add("cv", "Conversation");
            mappings.Add("d", "Description");
            mappings.Add("fn", "FriendlyName");
            mappings.Add("h", "Hash");
            mappings.Add("hp", "HashedPassword");
            mappings.Add("i", "Id");
            mappings.Add("mn", "MachineName");
            mappings.Add("n", "Name");
            mappings.Add("rtn", "RobotTypeName");
            mappings.Add("s", "Salt");
            mappings.Add("si", "SourceId");
            mappings.Add("smt", "SourceMessageType");
            mappings.Add("sn", "SourceName");
            mappings.Add("st", "SourceType");
            mappings.Add("t", "Type");
            mappings.Add("v", "Value");
            mappings.Add("vs", "Values");
            mappings.Add("wa", "WhenApplied");
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return mappings.TryGetValue(propertyName, out var value)
                ? value
                : base.ResolvePropertyName(propertyName);
        }
    }
}