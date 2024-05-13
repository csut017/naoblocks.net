namespace NaoBlocks.Common;

/// <summary>
/// Defines the commands that can be passed to a robot.
/// </summary>
public class RobotCommandList
{
    /// <summary>
    /// Initialize a new empty <see cref="RobotCommandList"/>.
    /// </summary>
    public RobotCommandList()
    {
    }

    /// <summary>
    /// Initialize a new <see cref="RobotCommandList"/> with an initial set of commands.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    public RobotCommandList(IEnumerable<string> commands)
    {
        Commands = commands.ToList();
        Count = Commands.Count;
    }

    /// <summary>
    /// The commands to pass.
    /// </summary>
    public List<string>? Commands { get; set; }

    /// <summary>
    /// The number of commands that should be added.
    /// </summary>
    public int Count { get; set; }
}