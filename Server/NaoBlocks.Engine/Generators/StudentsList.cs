using System.IO;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the students list export.
    /// </summary>
    public class StudentsList
        : ReportGenerator
    {
        /// <summary>
        /// Generates the students list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            throw new System.NotImplementedException();
        }
    }
}
