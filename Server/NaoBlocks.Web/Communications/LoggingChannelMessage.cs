namespace NaoBlocks.Web.Communications;

/// <summary>
/// A message for a <see cref="LoggingChannel"/>.
/// </summary>
/// <param name="Robot">The name of the robot.</param>
/// <param name="Action">The action being performed.</param>
/// <param name="Data">The data associated with the action.</param>
/// <param name="TimeStamp">The date and time the message was generated.</param>
public record LoggingChannelMessage(
    string Robot,
    string Action,
    string Data,
    DateTime TimeStamp);