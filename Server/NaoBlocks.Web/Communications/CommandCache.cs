using System.Collections.Concurrent;

namespace NaoBlocks.Web.Communications;

/// <summary>
/// Caches the commands for a pass-through robot (a robot that does not have any onboard command processing).
/// </summary>
public class CommandCache
{
    private readonly ConcurrentDictionary<string, CommandSet> robots = [];

    /// <summary>
    /// Gets a <see cref="CommandSet"/> instance for the named robot.
    /// </summary>
    /// <param name="robotName">The name of the robot.</param>
    /// <returns>A <see cref="CommandSet"/> instance.</returns>
    /// <remarks>
    /// Ths method will retrieve an existing <see cref="CommandSet"/> instance if there is one. Otherwise,
    /// it will generate a new <see cref="CommandSet"/> instance, add it to the cache, and return it.
    /// </remarks>
    public CommandSet Get(string robotName)
    {
        var set = new CommandSet { Name = robotName };
        return robots.GetOrAdd(robotName, set);
    }

    /// <summary>
    /// Lists all the current robots.
    /// </summary>
    /// <returns>The currently loaded robots.</returns>
    public IEnumerable<CommandSet> List()
    {
        return robots.Values;
    }

    /// <summary>
    /// The cache of commands for a robot.
    /// </summary>
    public class CommandSet
    {
        private readonly ConcurrentQueue<string> commands = [];

        private readonly object thisLock = new();

        private DateTime lastUpdated = DateTime.UtcNow;

        /// <summary>
        /// Gets the number of commands available.
        /// </summary>
        public int Count => commands.Count;

        /// <summary>
        /// Gets when this set was last updated.
        /// </summary>
        public DateTime LastUpdated
        {
            get { lock (thisLock) return lastUpdated; }
            private set { lock (thisLock) lastUpdated = value; }
        }

        /// <summary>
        /// The name of the robot.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Adds new commands to the command set.
        /// </summary>
        /// <param name="newCommands">The commands to add.</param>
        /// <returns>The total number of commands.</returns>
        public int Add(IEnumerable<string> newCommands)
        {
            foreach (var command in newCommands)
            {
                commands.Enqueue(command);
            }

            LastUpdated = DateTime.UtcNow;
            return commands.Count;
        }

        /// <summary>
        /// Clears all the current commands.
        /// </summary>
        /// <returns>The number of commands removed.</returns>
        public int Clear()
        {
            var count = commands.Count;
            commands.Clear();
            LastUpdated = DateTime.UtcNow;
            return count;
        }

        /// <summary>
        /// Retrieves commands from the set.
        /// </summary>
        /// <param name="maximum">The maximum number of commands to retrieve.</param>
        /// <returns>The retrieved commands.</returns>
        public IEnumerable<string> Get(int maximum)
        {
            var count = 0;
            while (count++ < maximum && commands.TryDequeue(out var command))
            {
                yield return command;
            }

            LastUpdated = DateTime.UtcNow;
        }
    }
}