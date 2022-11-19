using Microsoft.Extensions.Logging;
using System;
using System.Text;
using Xunit.Abstractions;

namespace NaoBlocks.Web.IntegrationTests
{
    public sealed class XunitLogger : ILogger
    {
        private const string Critical = "crit";
        private const string Debug = "dbug";
        private const string Error = "fail";
        private const string Info = "info";
        private const string ScopeDelimiter = "=> ";
        private const string Spacer = "      ";

        private const string Trace = "trce";
        private const string Warn = "warn";
        private readonly string categoryName;
        private readonly ITestOutputHelper output;
        private readonly IExternalScopeProvider? scopes;
        private readonly bool useScopes;

        public XunitLogger(ITestOutputHelper output, IExternalScopeProvider? scopes, string categoryName, bool useScopes)
        {
            this.output = output;
            this.scopes = scopes;
            this.categoryName = categoryName;
            this.useScopes = useScopes;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            if (this.scopes == null) return new Scope();
            return this.scopes.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();

            switch (logLevel)
            {
                case LogLevel.Trace:
                    sb.Append(Trace);
                    break;

                case LogLevel.Debug:
                    sb.Append(Debug);
                    break;

                case LogLevel.Information:
                    sb.Append(Info);
                    break;

                case LogLevel.Warning:
                    sb.Append(Warn);
                    break;

                case LogLevel.Error:
                    sb.Append(Error);
                    break;

                case LogLevel.Critical:
                    sb.Append(Critical);
                    break;

                case LogLevel.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }

            sb.Append(": ").Append(categoryName).Append('[').Append(eventId).Append(']').AppendLine();

            if (useScopes && TryAppendScopes(sb))
                sb.AppendLine();

            sb.Append(Spacer);
            sb.Append(formatter(state, exception));

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append(Spacer);
                sb.Append(exception);
            }

            var message = sb.ToString();
            try
            {
                output.WriteLine(message);
            }
            catch
            {
                // Sometimes we get errors here because we are trying to log after the test has completed
            }
        }

        private bool TryAppendScopes(StringBuilder sb)
        {
            var scopes = false;
            if (this.scopes != null)
            {
                this.scopes.ForEachScope((callback, state) =>
                {
                    if (!scopes)
                    {
                        state.Append(Spacer);
                        scopes = true;
                    }

                    state.Append(ScopeDelimiter);
                    state.Append(callback);
                }, sb);
            }
            return scopes;
        }

        public class Scope : IDisposable
        {
            private bool disposedValue;

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }

                    disposedValue = true;
                }
            }
        }
    }
}