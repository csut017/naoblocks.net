using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
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
        private readonly List<string> calledComponents = new();
        private readonly List<string> expectedComponents = new();

        public string Data { get; set; } = string.Empty;

        public IEnumerable<UIDefinitionItem> Description { get; set; } = Array.Empty<UIDefinitionItem>();

        public static FakeUIDefinition New(params UIDefinitionItem[] description)
        {
            return new FakeUIDefinition
            {
                Description = description
            };
        }

        public Task<IEnumerable<UIDefinitionItem>> DescribeAsync()
        {
            return Task.FromResult(this.Description);
        }

        public void ExpectGenerate(string component)
        {
            this.expectedComponents.Add(component);
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

        public void Verify()
        {
            Assert.Equal(this.expectedComponents, this.calledComponents);
        }
    }
}