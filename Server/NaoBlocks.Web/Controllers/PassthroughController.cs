using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;

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
    private static object mainLockObject = new();
    private static Dictionary<string, RobotCommandList> robotCommands = [];
    private readonly ILogger<CodeController> logger;

    /// <summary>
    /// Initialize a new <see cref="PassThroughController"/>.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    public PassThroughController(ILogger<CodeController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Deletes the current .
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
        while ((commands.GetCount() == 0) && (count > 0))
        {
            await Task.Delay(100);
            count--;
        }

        var commandString = commands.Retrieve();
        logger.LogInformation("Sending commands {commands} for {robot}", commandString, robot);
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

        List<KeyValuePair<string, RobotCommandList>> robots;
        lock (mainLockObject)
        {
            robots = robotCommands.ToList();
        }

        var allRobots = robots.ToDictionary(
            r => r.Key,
            r => r.Value.GetCount());
        logger.LogInformation("Retrieved {count} robots", allRobots.Count);
        return allRobots;
    }

    /// <summary>
    /// Adds additional commands to the stack for a robot.
    /// </summary>
    /// <param name="robot">The robot to add the commands to.</param>
    /// <param name="commandsToAdd">The commands to add.</param>
    /// <returns>An <see cref="ExecutionResult"/> containing the number of pending commands.</returns>
    [HttpPost("{robot}")]
    public ActionResult<ExecutionResult<Common.RobotCommandList>> Post(string robot, Common.RobotCommandList commandsToAdd)
    {
        logger.LogInformation("Adding commands for {robot}", robot);
        var commands = RetrieveRobot(robot);

        if (commandsToAdd.Commands == null)
        {
            logger.LogWarning("Could not add commands to {robot}: no commands to add", robot);
            return new BadRequestObjectResult(new ExecutionResult
            {
                ValidationErrors = new[] {
                    new CommandError(0, "No commands to add")
                }
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
                ValidationErrors = new[] {
                    new CommandError(0, "Command amount mismatch")
                }
            });
        }
        var count = commands.Add(newCommands);
        var commandString = string.Concat(',', commandsToAdd.Commands);
        logger.LogInformation("Added commands {commands} to {robot}, there are now {count} commands pending", robot, commandString, count);
        return ExecutionResult.New(new Common.RobotCommandList
        {
            Count = count
        });
    }

    private static RobotCommandList RetrieveRobot(string robot)
    {
        RobotCommandList? commands;
        lock (mainLockObject)
        {
            if (!robotCommands.TryGetValue(robot, out commands))
            {
                commands = new();
                robotCommands[robot] = commands;
            }
        }

        return commands;
    }

    private class RobotCommandList
    {
        private readonly List<string> commands = new();
        private readonly object lockObject = new();

        public int Add(string[] newCommands)
        {
            lock (lockObject)
            {
                commands.AddRange(newCommands);
                return commands.Count;
            }
        }

        public int Clear()
        {
            lock (lockObject)
            {
                var count = commands.Count;
                commands.Clear();
                return count;
            }
        }

        public int GetCount()
        {
            lock (lockObject)
            {
                return commands.Count;
            }
        }

        public string Retrieve()
        {
            lock (lockObject)
            {
                var currentCommands = string.Join("", commands);
                commands.Clear();
                return currentCommands;
            }
        }
    }
}