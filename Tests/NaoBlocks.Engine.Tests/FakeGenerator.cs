using System;
using System.IO;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    public class FakeGenerator
        : ReportGenerator
    {
        public bool IsCalled { get; private set; }

        public ReportFormat Format { get; private set; }

        public override Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            this.Format = format;
            this.IsCalled = true;
            return Task.FromResult(Tuple.Create((Stream)new MemoryStream(), "done"));
        }
    }
}
