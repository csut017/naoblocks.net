using System.Threading.Channels;

namespace NaoBlocks.Web.Communications;

/// <summary>
/// Defines a channel for logging incoming robot messages.
/// </summary>
public class LoggingChannel
    : ILoggingChannel
{
    private readonly Channel<LoggingChannelMessage> channel = Channel
        .CreateUnbounded<LoggingChannelMessage>(new()
        {
            SingleReader = true,
            SingleWriter = false,
        });

    /// <summary>
    /// The reader for the channel.
    /// </summary>
    public ChannelReader<LoggingChannelMessage> Reader => channel.Reader;

    /// <summary>
    /// The writer for the channel.
    /// </summary>
    public ChannelWriter<LoggingChannelMessage> Writer => channel.Writer;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
    }
}