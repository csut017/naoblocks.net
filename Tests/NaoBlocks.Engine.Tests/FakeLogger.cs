using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace NaoBlocks.Engine.Tests
{
    internal class FakeLogger<T>: ILogger<T>
    {
        public IList<string> Messages { get; } = new List<string>();

        public FakeLogger()
        {
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var msg = formatter(state, exception);
            this.Messages.Add($"{logLevel.ToString().ToUpperInvariant()}: {msg}");
        }
    }
}