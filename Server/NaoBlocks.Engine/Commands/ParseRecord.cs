namespace NaoBlocks.Engine.Commands
{
    internal static class ParseRecord
    {
        public static ParseRecord<TType> New<TType>(TType value, int row)
        {
            return new ParseRecord<TType>(value, row);
        }
    }

    internal class ParseRecord<TType>
    {
        public ParseRecord(TType value, int row)
        {
            this.Value = value;
            this.Row = row;
        }

        public int Row { get; }

        public TType Value { get; }
    }
}