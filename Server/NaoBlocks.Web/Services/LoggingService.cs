using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Communications;
using Raven.Client.Documents;

namespace NaoBlocks.Web.Services;

/// <summary>
/// Logs in-coming robot messages.
/// </summary>
public class LoggingService(IServiceProvider serviceProvider, ILogger<LoggingService> logger, ILoggingChannel loggingChannel)
    : IHostedService
{
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private CancellationToken? token;

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous Start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        token = cancellationTokenSource.Token;
        Task.Run(Start, cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous Stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Shutting down logging service");
        cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private record RobotConversation(Robot Robot, Conversation Value);

    private async Task Start()
    {
        if (!token.HasValue) throw new InvalidOperationException("There is no cancellation token");

        logger.LogInformation("Starting logging service");
        var scope = serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IDatabase>();
        var robots = new Dictionary<string, RobotConversation?>();
        while (await loggingChannel.Reader.WaitToReadAsync(token.Value))
        {
            logger.LogInformation("Processing logging messages");
            using var session = database.StartSession();
            while (loggingChannel.Reader.TryRead(out var message))
            {
                var justStarted = false;
                var robotName = message.Robot.ToLowerInvariant();
                if (!robots.TryGetValue(robotName, out var conversation))
                {
                    var robot = await session.Query<Robot>()
                        .FirstOrDefaultAsync(r => r.MachineName == message.Robot, token.Value);
                    if (robot != null)
                    {
                        conversation = await StartConversation(session, robot, robots, robotName);
                        justStarted = true;
                    }
                }

                if (conversation == null)
                {
                    logger.LogInformation("Unknown robot '{robot}' - skipping log", message.Robot);
                    continue;
                }

                if (message.Action == "start")
                {
                    if (!justStarted) await StartConversation(session, conversation.Robot, robots, robotName);
                    continue;
                }

                var log = new RobotLog
                {
                    WhenAdded = DateTime.UtcNow,
                    WhenLastUpdated = DateTime.UtcNow,
                    RobotId = conversation.Robot.Id,
                    Conversation = conversation.Value,
                };
                var line = new RobotLogLine
                {
                    Description = message.Action,
                    ClientMessageType = message.Action,
                    WhenAdded = DateTime.UtcNow,
                };
                line.Values.Add(new NamedValue
                {
                    Name = "data",
                    Value = message.Data
                });
                log.Lines.Add(line);
                await session.StoreAsync(log);
                logger.LogInformation("Added log for robot '{robot}'", conversation.Robot.MachineName);
            }

            await session.SaveChangesAsync();
        }
    }

    private async Task<RobotConversation> StartConversation(IDatabaseSession session, Robot robot, Dictionary<string, RobotConversation?> robots, string robotName)
    {
        logger.LogInformation("Starting conversation for robot '{robot}'", robot.MachineName);
        var systemValues = await session.Query<SystemValues>().FirstOrDefaultAsync();
        if (systemValues == null)
        {
            systemValues = new SystemValues();
            await session.StoreAsync(systemValues);
        }
        var conversationId = ++systemValues.NextConversationId;
        var details = new Conversation
        {
            ConversationId = conversationId,
            ConversationType = ConversationType.Logging,
            SourceId = robot.Id,
            SourceName = robot.MachineName,
            SourceType = "Robot"
        };
        await session.StoreAsync(details);

        var conversation = new RobotConversation(robot, details);
        robots[robotName] = conversation;
        return conversation;
    }
}