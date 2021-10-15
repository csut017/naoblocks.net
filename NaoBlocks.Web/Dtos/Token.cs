namespace NaoBlocks.Web.Dtos
{
    public class Token
    {
        public static Token Empty
        {
            get { return new Token(); }
        }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public static Token? FromModel(Common.Token? value)
        {
            return value == null ? null : new Token
            {
                LineNumber = value.LineNumber,
                LinePosition = value.LinePosition,
                Type = value.Type.ToString(),
                Value = value.Value
            };
        }
    }
}