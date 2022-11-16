using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace NaoBlocks.Web.IntegrationTests
{
    public sealed class XunitLoggerProvider
        : ILoggerProvider, ISupportExternalScope
    {
        private readonly ITestOutputHelper _output;
        private readonly bool _useScopes;

        private IExternalScopeProvider? scopes;

        public XunitLoggerProvider(ITestOutputHelper output, bool useScopes)
        {
            _output = output;
            _useScopes = useScopes;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, this.scopes, categoryName, _useScopes);
        }

        public void Dispose()
        {
        }

        public void SetScopeProvider(IExternalScopeProvider scopes)
        {
            this.scopes = scopes;
        }
    }
}