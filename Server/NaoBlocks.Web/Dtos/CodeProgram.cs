using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// An uncompiled program.
    /// </summary>
    public class CodeProgram
    {
        /// <summary>
        /// Gets or sets the NaoLang program code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the program.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the source of the code (e.g. Blockly, TopCodes, etc.)
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets whether the program should be stored or not.
        /// </summary>
        public bool? Store { get; set; }

        /// <summary>
        /// Gets or sets when the program was added.
        /// </summary>
        public DateTime WhenAdded { get; set; }

        /// <summary>
        /// Converts from a data entity to a data transfer object.
        /// </summary>
        /// <param name="value">The <see cref="Data.CodeProgram"/> to convert.</param>
        /// <param name="includeDetails">Whether to include the details or not.</param>
        /// <returns>A <see cref="CodeProgram"/> instance.</returns>
        public static CodeProgram FromModel(Data.CodeProgram value, bool includeDetails = false)
        {
            var program = new CodeProgram
            {
                Id = value.Number,
                Name = value.Name,
                WhenAdded = value.WhenAdded
            };

            if (includeDetails) program.Code = value.Code;

            return program;
        }
    }
}