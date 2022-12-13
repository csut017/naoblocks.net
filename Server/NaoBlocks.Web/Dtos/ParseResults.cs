using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Includes details on the parsing results.
    /// </summary>
    public class ParseResults
    {
        /// <summary>
        /// Gets the details of the parse.
        /// </summary>
        public IDictionary<string, object> Details { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="ParseResults"/> instance containing the required properties.</returns>
        public static ParseResults FromModel<TItem>(Data.ItemImport<TItem> value)
            where TItem : class
        {
            var output = new ParseResults { Message = value.Message ?? string.Empty };
            output.Details["duplicate"] = value.IsDuplicate;
            output.Details["canImport"] = value.CanImport;
            return output;
        }
    }
}