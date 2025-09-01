using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
{
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
        loggingChannel.Writer.TryWrite(new LoggingChannelMessage(robot, "send", commandString, DateTime.UtcNow));
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

    private CommandCache.CommandSet RetrieveRobot(string robot)
    {
        return commandCache.Get(robot);
    }
}