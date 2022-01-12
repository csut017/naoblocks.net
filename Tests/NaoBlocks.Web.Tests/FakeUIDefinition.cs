using NaoBlocks.Common;
using NaoBlocks.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests
{
    public class FakeUIDefinition
        : IUIDefinition
    {
        private readonly List<string> expectedComponents = new();
        private readonly List<string> calledComponents = new();

        public string Data { get; set; } = string.Empty;

        public void ExpectGenerate(string component)
        {
            this.expectedComponents.Add(component);
        }

        public void Verify()
        {
            Assert.Equal(this.expectedComponents, this.calledComponents);
        }

        public Task<Stream> GenerateAsync(string component)
        {
            this.calledComponents.Add(component);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(this.Data));
            return Task.FromResult((Stream)stream);
        }

        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            throw new NotImplementedException();
        }
    }
}
