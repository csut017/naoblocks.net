using NaoBlocks.Common;

namespace NaoBlocks.Communications.Tests
{
    internal static class TestHelpers
    {
        public static string GenerateDataString(this ClientMessage message)
        {
            return string.Join(",", message.Values.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        public static void PopulateMessageData(this ClientMessage message, string? data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var pair in data.Split(","))
                {
                    var parts = pair.Split('=');
                    message.Values[parts[0]] = parts[1];
                }
            }
        }
    }
}