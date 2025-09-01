using System.Threading.Channels;

namespace NaoBlocks.Web.Communications;

/// <summary>
/// Defines a channel for passing logging messages about a robot.
/// </summary>
public interface ILoggingChannel
    : IDisposable
{
    /// <summary>
    /// The reader for the channel.
    /// </summary>
    ChannelReader<LoggingChannelMessage> Reader { get; }

    /// <summary>
    /// The writer for the channel.
    /// </summary>
    ChannelWriter<LoggingChannelMessage> Writer { get; }
}