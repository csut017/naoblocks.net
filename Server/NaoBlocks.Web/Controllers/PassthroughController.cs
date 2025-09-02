using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NaoBlocks.Common;
using NaoBlocks.Web.Communications;

namespace NaoBlocks.Web.Controllers;

/// <summary>
/// Controller for passing through robot commands.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[AllowAnonymous]
[Produces("application/json")]
public class PassThroughController
    : ControllerBase
{
    private static readonly ReadOnlyMemory<byte> DataEnd = new("\n"u8.ToArray());
    private static readonly ReadOnlyMemory<byte> DataStart = new("data: "u8.ToArray());
    private static readonly ReadOnlyMemory<byte> EmptyData = new("data: {}"u8.ToArray());
    private static readonly ReadOnlyMemory<byte> EventSeparator = new("\n\n"u8.ToArray());
    private static readonly ReadOnlyMemory<byte> MessageType = new("event: message\n"u8.ToArray());
    private static readonly ReadOnlyMemory<byte> PingType = new("event: ping\n"u8.ToArray());
    private static readonly object SubscriptionLock = new object();
    private static readonly ConcurrentDictionary<string, List<ILoggingChannel>> Subscriptions = [];
    private readonly CommandCache commandCache;
    private readonly ILogger<PassThroughController> logger;
    private readonly ILoggingChannel loggingChannel;

    /// <summary>
    /// Initialize a new <see cref="PassThroughController"/>.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="commandCache">The associated command cache.</param>
    /// <param name="loggingChannel">The <see cref="ILoggingChannel"/> instance to use for logging.</param>
    public PassThroughController(ILogger<PassThroughController> logger, CommandCache commandCache, ILoggingChannel loggingChannel)
    {
        this.logger = logger;
        this.commandCache = commandCache;
        this.loggingChannel = loggingChannel;
    }

    /// <summary>
    /// Deletes the current commands for a robot.
    /// </summary>
    /// <param name="robot">The robot to retrieve the commands for.</param>
    /// <returns>A <see cref="string"/> containing all the pending commands for the robot.</returns>
    [HttpDelete("{robot}")]
    public ActionResult Delete(string robot)
    {
        logger.LogInformation("Clearing commands for {robot}", robot);
        var commands = RetrieveRobot(robot);

        var count = commands.Clear();
        logger.LogInformation("Cleared {count} commands from {robot}", count, robot);
        loggingChannel.Writer.TryWrite(new LoggingChannelMessage(robot, "clear", string.Empty, DateTime.UtcNow));
        return new NoContentResult();
    }

    /// <summary>
    /// Retrieves the pending commands for a robot.
    /// </summary>
    /// <param name="robot">The robot to retrieve the commands for.</param>
    /// <param name="maxDelay">The maximum number of seconds to check for results.</param>
    /// <returns>A <see cref="string"/> containing all the pending commands for the robot.</returns>
    [HttpGet("{robot}")]
    public async Task<ActionResult<string>> Get(string robot, int maxDelay = 5)
    {
        logger.LogInformation("Getting commands for {robot}", robot);
        var commands = RetrieveRobot(robot);

        var count = 10 * maxDelay;
        while (commands.Count == 0 && count-- > 0)
        {
            await Task.Delay(100);
        }

        var commandString = string.Join(string.Empty, commands.Get(10));
        logger.LogInformation("Sending commands {commands} for {robot}", commandString, robot);
        var message = new LoggingChannelMessage(robot, "send", commandString, DateTime.UtcNow);
        loggingChannel.Writer.TryWrite(message);

        // Send to the subscriptions
        if (Subscriptions.TryGetValue(robot.ToLowerInvariant(), out var list))
        {
            logger.LogInformation("Sending to subscribers for {robot}", robot);
            lock (SubscriptionLock)
            {
                foreach (var channel in list)
                {
                    channel.Writer.TryWrite(message);
                }
            }
        }

        return new ContentResult
        {
            Content = commandString
        };
    }

    /// <summary>
    /// Retrieves the pending commands for a robot.
    /// </summary>
    /// <returns>All the robots and the number of commands they contain.</returns>
    [HttpGet("status")]
    public ActionResult<Dictionary<string, int>> GetStatus()
    {
        logger.LogInformation("Retrieving pass-through status");

        var robots = commandCache.List();
        var allRobots = robots.ToDictionary(
            r => r.Name,
            r => r.Count);
        logger.LogInformation("Retrieved {count} robots", allRobots.Count);
        return allRobots;
    }

    /// <summary>
    /// Adds additional commands to the stack for a robot.
    /// </summary>
    /// <param name="robot">The robot to add the commands to.</param>
    /// <param name="commandsToAdd">The commands to add.</param>
    /// <returns>An <see cref="ExecutionResult"/> containing the number of pending commands.</returns>
    [HttpPost("{robot}/{commandsToAdd}")]
    public ActionResult<ExecutionResult<RobotCommandList>> Post(string robot, string commandsToAdd)
    {
        var commands = commandsToAdd.Select(c => new string(c, 1)).ToList();
        return Post(robot, new RobotCommandList
        {
            Commands = commands,
            Count = commands.Count
        });
    }

    /// <summary>
    /// Adds additional commands to the stack for a robot.
    /// </summary>
    /// <param name="robot">The robot to add the commands to.</param>
    /// <param name="commandsToAdd">The commands to add.</param>
    /// <returns>An <see cref="ExecutionResult"/> containing the number of pending commands.</returns>
    [HttpPost("{robot}")]
    public ActionResult<ExecutionResult<RobotCommandList>> Post(string robot, RobotCommandList commandsToAdd)
    {
        logger.LogInformation("Adding commands for {robot}", robot);
        var commands = RetrieveRobot(robot);

        if (commandsToAdd.Commands == null)
        {
            logger.LogWarning("Could not add commands to {robot}: no commands to add", robot);
            return new BadRequestObjectResult(new ExecutionResult
            {
                ValidationErrors =
                [
                    new CommandError(0, "No commands to add")
                ]
            });
        }

        var newCommands = commandsToAdd.Commands!.ToArray();
        if ((commandsToAdd.Count >= 0) && (commandsToAdd.Count != newCommands.Length))
        {
            logger.LogWarning("Could not add commands to {robot}: number of commands does not match - expected {expected}, found {actual}",
                robot,
                commandsToAdd.Count,
                newCommands.Length);
            return new BadRequestObjectResult(new ExecutionResult
            {
                ValidationErrors =
                [
                    new CommandError(0, "Command amount mismatch")
                ]
            });
        }
        var count = commands.Add(newCommands);
        var commandString = string.Concat(newCommands);
        logger.LogInformation("Added commands {commands} to {robot}, there are now {count} commands pending", commandString, robot, count);
        loggingChannel.Writer.TryWrite(new LoggingChannelMessage(robot, "receive", commandString, DateTime.UtcNow));
        return ExecutionResult.New(new RobotCommandList
        {
            Count = count
        });
    }

    /// <summary>
    /// Starts a robot session.
    /// </summary>
    /// <param name="robot">The robot to start the session for.</param>
    /// <returns>An Ok result.</returns>
    [HttpPost("{robot}/start")]
    public ActionResult Start(string robot)
    {
        loggingChannel.Writer.TryWrite(new LoggingChannelMessage(robot, "start", string.Empty, DateTime.UtcNow));
        return new OkResult();
    }

    /// <summary>
    /// Subscribe to notifications of when a robot changes.
    /// </summary>
    /// <param name="robot">The name of the robot.</param>
    /// <param name="token">The cancellation token.</param>
    [HttpGet("{robot}/subscribe")]
    public async Task Subscribe(string robot, CancellationToken token)
    {
        logger.LogInformation("starting subscription for {robot}", robot);
        Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

        var robotName = robot.ToLowerInvariant();
        if (!Subscriptions.TryGetValue(robotName, out var list))
        {
            list = [];
            if (!Subscriptions.TryAdd(robotName, list)) list = Subscriptions[robotName];
        }
        var channel = new LoggingChannel();
        lock (SubscriptionLock)
            list.Add(channel);

        while (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(30), token);
            var channelRead = channel.Reader.WaitToReadAsync(token).AsTask();
            await Task.WhenAny(timeout, channelRead);
            if (token.IsCancellationRequested) break;
            if (timeout.IsCompleted)
            {
                await Response.Body.WriteAsync(PingType, token);
                await Response.Body.WriteAsync(EmptyData, token);
                await Response.Body.WriteAsync(EventSeparator, token);
            }
            else
            {
                while (channel.Reader.TryRead(out var message))
                {
                    foreach (var command in message.Data)
                    {
                        await Response.Body.WriteAsync(MessageType, token);
                        await Response.Body.WriteAsync(DataStart, token);
                        await JsonSerializer.SerializeAsync(Response.Body, new { command }, JsonSerializerOptions.Default, token);
                        await Response.Body.WriteAsync(DataEnd, token);
                        await Response.Body.WriteAsync(EventSeparator, token);
                        await Response.Body.FlushAsync(token);
                        await Task.Delay(TimeSpan.FromSeconds(1), token);
                    }
                }
            }
            await Response.Body.FlushAsync(token);
        }

        logger.LogInformation("Subscription finished for {robot}", robot);
        lock (SubscriptionLock)
            list.Remove(channel);
    }

    private CommandCache.CommandSet RetrieveRobot(string robot)
    {
        return commandCache.Get(robot);
    }
}