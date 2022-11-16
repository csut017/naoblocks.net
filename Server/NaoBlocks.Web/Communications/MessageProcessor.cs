using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Processes incoming messages.
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors;
        private readonly IEngineFactory engineFactory;
        private readonly IHub hub;
        private readonly ILogger<MessageProcessor> logger;

        /// <summary>
        /// Initialise a new <see cref="MessageProcessor"/> instance.
        /// </summary>
        /// <param name="hub">The <see cref="IHub"/> instance to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="engineFactory">The <see cref="IEngineFactory"/> to use.</param>
        public MessageProcessor(IHub hub, ILogger<MessageProcessor> logger, IEngineFactory engineFactory)
        {
            this._processors = new Dictionary<ClientMessageType, TypeProcessor>
            {
                { ClientMessageType.Authenticate, this.Authenticate },
                { ClientMessageType.RequestRobot, this.AllocateRobot },
                { ClientMessageType.TransferProgram, this.TransferProgramToRobot },
                { ClientMessageType.StartProgram, this.StartProgram },
                { ClientMessageType.StopProgram, this.StopProgram },
                { ClientMessageType.ProgramDownloaded, this.ProgramDownloaded },
                { ClientMessageType.ProgramStarted, this.BroadcastMessage(ClientMessageType.ProgramStarted, "Program started") },
                { ClientMessageType.ProgramFinished, this.BroadcastMessage(ClientMessageType.ProgramFinished, "Program finished") },
                { ClientMessageType.ProgramStopped, this.BroadcastMessage(ClientMessageType.ProgramStopped, "Program stopped") },
                { ClientMessageType.RobotDebugMessage, this.RobotDebugMessage },
                { ClientMessageType.RobotError, this.BroadcastMessage(ClientMessageType.RobotError, "An unexpected error has occurred", true) },
                { ClientMessageType.RobotStateUpdate, this.UpdateRobotState },
                { ClientMessageType.UnableToDownloadProgram, this.BroadcastMessage(ClientMessageType.UnableToDownloadProgram, "Unable to download program", true) },
                { ClientMessageType.StartMonitoring, this.StartMonitoringAllClients },
                { ClientMessageType.StopMonitoring, this.StopMonitoringAllClients },
                { ClientMessageType.AlertsRequest, this.HandleAlertsRequest},
                { ClientMessageType.AlertBroadcast, this.HandleBroadcastAlert }
            };
            this.hub = hub;
            this.logger = logger;
            this.engineFactory = engineFactory;
        }

        /// <summary>
        /// The definition of a message processor (for a specific a message type.)
        /// </summary>
        /// <param name="engine">The engine to use when processing a message.</param>
        /// <param name="client">The source client.</param>
        /// <param name="message">The incoming message.</param>
        /// <returns></returns>
        private delegate Task TypeProcessor(IExecutionEngine engine, IClientConnection client, ClientMessage message);

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger<MessageProcessor> Logger => logger;

        /// <summary>
        /// Processes a message.
        /// </summary>
        /// <param name="client">The client that received the message.</param>
        /// <param name="message">The message to process.</param>
        public async Task ProcessAsync(IClientConnection client, ClientMessage message)
        {
            if (this._processors.TryGetValue(message.Type, out TypeProcessor? processor) && (processor != null))
            {
                try
                {
                    var (engine, session) = this.engineFactory.Initialise();
                    try
                    {
                        this.Logger.LogInformation($"Processing message type {message.Type}");
                        await processor(engine, client, message);
                        await session.SaveChangesAsync();
                    }
                    finally
                    {
                        session.Dispose();
                    }
                }
                catch (Exception err)
                {
                    this.Logger.LogWarning($"An error occurred while processing {message.Type}: {err.Message}");
                    client.SendMessage(GenerateErrorResponse(message, $"Unable to process message: {err.Message}"));
                }
            }
            else
            {
                this.Logger.LogWarning($"Unable to find processor for message type {message.Type}");
                client.SendMessage(GenerateErrorResponse(message, $"Unable to find processor for {message.Type}"));
            }
        }

        /// <summary>
        /// Generates an error message in response to an incoming message.
        /// </summary>
        /// <param name="request">The incoming message.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A new <see cref="ClientMessage"/> containing the error response.</returns>
        private static ClientMessage GenerateErrorResponse(ClientMessage request, string message)
        {
            var response = new ClientMessage
            {
                Type = ClientMessageType.Error,
                ConversationId = request.ConversationId
            };
            response.Values["error"] = message;
            return response;
        }

        /// <summary>
        /// Generates an empty response message.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="type">The type of response.</param>
        /// <returns>A <see cref="ClientMessage"/> containing the response.</returns>
        private static ClientMessage GenerateResponse(ClientMessage request, ClientMessageType type)
        {
            var response = new ClientMessage
            {
                Type = type,
                ConversationId = request.ConversationId
            };
            return response;
        }

        /// <summary>
        /// Populates values in the <see cref="ClientMessage"/> based on the client.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to retrieve the values from.</param>
        /// <param name="msg">The <see cref="ClientMessage"/> to populate.</param>
        private static void PopulateSourceValues(IClientConnection client, ClientMessage msg)
        {
            msg.Values["SourceClientId"] = client.Id.ToString(CultureInfo.InvariantCulture);
            switch (client.Type)
            {
                case ClientConnectionType.Robot:
                    msg.Values["SourceType"] = "Robot";
                    if (client.Robot != null)
                    {
                        msg.Values["SourceName"] = client.Robot.MachineName;
                    }
                    break;

                case ClientConnectionType.User:
                    msg.Values["SourceType"] = "User";
                    if (client.User != null)
                    {
                        msg.Values["SourceName"] = client.User.Name;
                    }
                    break;

                default:
                    msg.Values["SourceType"] = "Unknown";
                    break;
            }
        }

        /// <summary>
        /// Validates and executes a command.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <param name="errors">The errors from the process.</param>
        /// <param name="command">The command to execute.</param>
        private static async Task<CommandResult> ValidateAndExecuteCommand(IExecutionEngine engine, List<CommandError> errors, CommandBase command)
        {
            errors.AddRange(await engine.ValidateAsync(command));
            if (!errors.Any())
            {
                var result = await engine.ExecuteAsync(command);
                if (!result.WasSuccessful)
                {
                    errors.AddRange(result.ToErrors());
                }
                return result;
            }

            return new CommandResult(command.Number, "Validation failed");
        }

        /// <summary>
        /// Validates the incoming request.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> instance the request is coming in on.</param>
        /// <param name="message">The message to validate.</param>
        /// <param name="requiredRole">The required role, if any.</param>
        /// <returns>True if the request is valid, false otherwise.</returns>
        private static bool ValidateRequest(IClientConnection client, ClientMessage message, ClientConnectionType? requiredRole = null)
        {
            if ((client.User == null) && (client.Robot == null))
            {
                client.SendMessage(GenerateResponse(message, ClientMessageType.NotAuthenticated));
                return false;
            }

            if (requiredRole.HasValue)
            {
                switch (requiredRole.Value)
                {
                    case ClientConnectionType.Robot:
                        if (client.Robot == null)
                        {
                            client.SendMessage(GenerateResponse(message, ClientMessageType.Forbidden));
                            return false;
                        }

                        break;

                    case ClientConnectionType.User:
                        if (client.User == null)
                        {
                            client.SendMessage(GenerateResponse(message, ClientMessageType.Forbidden));
                            return false;
                        }

                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates a logline for a robot and adds it to a robot's log.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <param name="machineName">The identifier of the robot to add the line to.</param>
        /// <param name="message">The message that the line is being generated as a result of.</param>
        /// <param name="description">The description of the line.</param>
        /// <param name="skipValues">Whether to include the values or not.</param>
        /// <param name="conversation">An optional conversation to associate.</param>
        private async Task<IEnumerable<CommandError>> AddToRobotLogAsync(IExecutionEngine engine, string machineName, ClientMessage message, string description, bool skipValues = false, Conversation? conversation = null)
        {
            var errors = new List<CommandError>();
            if (message.ConversationId == null)
            {
                errors.Add(new CommandError(-1, "Adding to a robot log requires a conversation"));
            }

            if (!errors.Any())
            {
                var command = new AddToRobotLog
                {
                    ConversationId = message.ConversationId!.Value,
                    MachineName = machineName,
                    Description = description,
                    SourceMessageType = message.Type,
                    Conversation = conversation
                };

                if (!skipValues)
                {
                    foreach (var (key, value) in message.Values)
                    {
                        command.Values.Add(new NamedValue
                        {
                            Name = key,
                            Value = value
                        });
                    }
                }

                await ValidateAndExecuteCommand(engine, errors, command);
            }

            foreach (var error in errors)
            {
                this.Logger.LogWarning($"Broadcast error: {error.Error}");
            }

            return errors;
        }

        /// <summary>
        /// Attempts to allocate a robot.
        /// </summary>
        private async Task AllocateRobot(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;

            logger.LogInformation($"Attempting to allocate robot for {client.User?.Name}");
            IClientConnection? nextRobot = null;
            if ((client.User?.Settings != null) && (client.User.Settings.AllocationMode > 0))
            {
                nextRobot = this.hub.GetClients(ClientConnectionType.Robot)
                    .FirstOrDefault(r => (r.Robot != null) && (r.Robot.MachineName == client.User.Settings.RobotId) && r.Status.IsAvailable);
                if ((nextRobot == null) && (client.User.Settings.AllocationMode == 1))
                {
                    this.logger.LogInformation($"Robot {client.User.Settings.RobotId} is not available for allocation");
                    client.SendMessage(GenerateResponse(message, ClientMessageType.NoRobotsAvailable));
                    return;
                }
            }

            if (nextRobot == null)
            {
                this.logger.LogDebug($"Randomly selecting robot");
                var rnd = new Random();
                nextRobot = this.hub.GetClients(ClientConnectionType.Robot)
                    .OrderBy(r => r.Status.LastAllocatedTime)
                    .ThenBy(_ => rnd.Next())
                    .FirstOrDefault(r => r.Status.IsAvailable);
            }

            if (nextRobot?.Robot == null)
            {
                this.logger.LogInformation($"No robots available for allocation");
                client.SendMessage(GenerateResponse(message, ClientMessageType.NoRobotsAvailable));
                logger.LogInformation($"Unable to allocate robot for {client.User?.Name}");
                return;
            }

            logger.LogInformation($"Allocated robot {nextRobot.Robot.MachineName} to {client.User?.Name}");
            nextRobot.Status.IsAvailable = false;
            nextRobot.Status.LastAllocatedTime = DateTime.UtcNow;
            nextRobot.AddListener(client);
            var response = GenerateResponse(message, ClientMessageType.RobotAllocated);
            response.Values["robot"] = nextRobot.Id.ToString(CultureInfo.InvariantCulture);
            client.SendMessage(response);

            client.LogMessage(response);
            PopulateSourceValues(client, response);
            this.hub.SendToMonitors(response);

            this.logger.LogInformation($"Allocated robot {nextRobot.Id}");
            await AddToRobotLogAsync(engine, nextRobot.Robot.MachineName, message, "Robot allocated to user");
        }

        /// <summary>
        /// Attempts to authenticate a client (user or robot.)
        /// </summary>
        private async Task Authenticate(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            this.logger.LogInformation("Authenticating");
            if (!message.Values.TryGetValue("token", out string? token))
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is missing"));
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is invalid"));
                return;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var sessionId = jwtToken.Claims.FirstOrDefault(c => c.Type == "SessionId");
            if (sessionId == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is invalid: missing session"));
                return;
            }

            var userSession = await engine.Query<SessionData>()
                .RetrieveByIdAsync(sessionId.Value)
                .ConfigureAwait(false);
            if ((userSession == null) || (userSession.WhenExpires < DateTime.UtcNow))
            {
                client.SendMessage(GenerateErrorResponse(message, "Session is invalid"));
                return;
            }

            var msg = new ClientMessage
            {
                Type = ClientMessageType.ClientAdded
            };
            msg.Values["ClientId"] = client.Id.ToString(CultureInfo.InvariantCulture);
            msg.Values["Type"] = "Unknown";

            if (userSession.UserId == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Session is invalid: missing id"));
                return;
            }

            if (userSession.IsRobot)
            {
                client.Robot = await engine.Query<RobotData>()
                    .RetrieveByIdAsync(userSession.UserId)
                    .ConfigureAwait(false);
                if (client.Robot == null)
                {
                    client.SendMessage(GenerateErrorResponse(message, "Session is invalid: missing robot"));
                    return;
                }

                this.logger.LogInformation($"Authenticated robot {client.Robot.MachineName}");
                var conversation = await this.StartConversation(message, client, engine, new StartRobotConversation
                {
                    Name = client.Robot.MachineName,
                    Type = ConversationType.Initialisation
                });
                if (conversation == null)
                {
                    return;
                }

                await AddToRobotLogAsync(engine, client.Robot.MachineName, message, "Robot authenticated", false, conversation);
                msg.Values["Type"] = "robot";
                msg.Values["SubType"] = client.Robot!.Type?.Name ?? "Unknown";
                msg.Values["Name"] = client.Robot!.FriendlyName;
            }
            else
            {
                client.User = await engine.Query<UserData>()
                    .RetrieveByIdAsync(userSession.UserId)
                    .ConfigureAwait(false);
                if (client.User == null)
                {
                    client.SendMessage(GenerateErrorResponse(message, "Session is invalid: missing user"));
                    return;
                }

                this.logger.LogInformation($"Authenticated user {client.User.Name}");
                var conversation = await this.StartConversation(message, client, engine, new StartUserConversation
                {
                    Name = client.User.Name,
                    Type = ConversationType.Program
                });
                if (conversation == null)
                {
                    return;
                }

                msg.Values["Type"] = "user";
                msg.Values["SubType"] = client.User.Role.ToString();
                msg.Values["Name"] = client.User.Name;
                msg.Values["IsStudent"] = client.User.Role == UserRole.Student ? "yes" : "no";
            }

            client.LogMessage(msg);
            this.hub.SendToMonitors(msg);
            client.SendMessage(GenerateResponse(message, ClientMessageType.Authenticated));
        }

        /// <summary>
        /// Initialises a message broadcast processor.
        /// </summary>
        /// <param name="messageType">The type of message to broadcast.</param>
        /// <param name="logDescription">The description of the message.</param>
        /// <param name="includeValues">Whether to include the values from the source or not.</param>
        /// <returns>The <see cref="TypeProcessor"/> for sending the message.</returns>
        private TypeProcessor BroadcastMessage(ClientMessageType messageType, string logDescription, bool includeValues = false)
        {
            return async (IExecutionEngine engine, IClientConnection client, ClientMessage message) =>
            {
                var valuesToCopy = includeValues ? message.Values : null;
                await this.DoBroadcastMessage(engine, client, message, messageType, logDescription, valuesToCopy);
            };
        }

        /// <summary>
        /// Performs the actual broadcast of the message.
        /// </summary>
        private async Task DoBroadcastMessage(IExecutionEngine engine, IClientConnection client, ClientMessage message, ClientMessageType messageType, string logDescription, IDictionary<string, string>? valuesToCopy)
        {
            var msg = GenerateResponse(message, messageType);
            if (valuesToCopy != null)
            {
                foreach (var (key, value) in valuesToCopy)
                {
                    msg.Values[key] = value;
                }
            }

            client.NotifyListeners(msg);
            client.LogMessage(msg);
            PopulateSourceValues(client, msg);
            this.hub.SendToMonitors(msg);
            if (client.Robot != null)
            {
                await AddToRobotLogAsync(engine, client.Robot.MachineName, message, logDescription);
            }
        }

        /// <summary>
        /// Handles an alert message.
        /// </summary>
        private Task HandleAlertsRequest(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return Task.CompletedTask;
            if (!this.RetrieveRobot(client, message, out IClientConnection? robotClient)) return Task.CompletedTask;

            foreach (var alert in robotClient!.Notifications)
            {
                var broadcast = new ClientMessage(ClientMessageType.AlertBroadcast, new
                {
                    id = alert.Id,
                    message = alert.Message,
                    severity = alert.Severity
                });
                PopulateSourceValues(robotClient, broadcast);
                client.SendMessage(broadcast);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles a broadcast message.
        /// </summary>
        private Task HandleBroadcastAlert(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.Robot)) return Task.CompletedTask;

            var severityText = message.Values.TryGetValue("severity", out string? severity) ? severity : "info";
            var alert = new NotificationAlert
            {
                Id = int.TryParse(message.Values.TryGetValue("id", out string? value) ? value : "-1", out int idValue) ? idValue : -1,
                Message = message.Values.TryGetValue("message", out string? msg) ? msg : string.Empty,
                Severity = string.IsNullOrEmpty(severityText) ? "info" : severityText,
                WhenAdded = DateTime.UtcNow
            };

            client.AddNotification(alert);
            var broadcast = new ClientMessage(ClientMessageType.AlertBroadcast, new
            {
                id = alert.Id,
                message = alert.Message,
                severity = alert.Severity
            });
            PopulateSourceValues(client, broadcast);
            this.hub.SendToMonitors(broadcast);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles a program downloaded message.
        /// </summary>
        private async Task ProgramDownloaded(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            var valuesToCopy = new Dictionary<string, string>
            {
                { "ProgramId", client.RobotDetails?.LastProgramId?.ToString(CultureInfo.InvariantCulture) ?? "0" }
            };
            await this.DoBroadcastMessage(engine, client, message, ClientMessageType.ProgramTransferred, "Program has been transferred", valuesToCopy);
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="IClientConnection"/> instance to the robot.
        /// </summary>
        /// <param name="client">The client attempting to get the robot connection.</param>
        /// <param name="message">The source message (used to generate any errors).</param>
        /// <param name="robotClient">The retrieved <see cref="IClientConnection"/> instance if found, null otherwise.</param>
        /// <returns>True if the robot could be retrieved, false otherwise.</returns>
        private bool RetrieveRobot(IClientConnection client, ClientMessage message, out IClientConnection? robotClient)
        {
            robotClient = null;
            if (!message.Values.TryGetValue("robot", out string? robotCode))
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot is missing"));
                return false;
            }

            if (!int.TryParse(robotCode, out int robotId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot id is invalid"));
                return false;
            }

            robotClient = this.hub.GetClient(robotId);
            if (robotClient == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot is no longer connected"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Broadcasts a debug message.
        /// </summary>
        private async Task RobotDebugMessage(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            await this.DoBroadcastMessage(engine, client, message, ClientMessageType.RobotDebugMessage, "Debug information received", message.Values);
            if (client.RobotDetails != null)
            {
                if (message.Values.TryGetValue("sourceID", out string? sourceId) && !string.IsNullOrWhiteSpace(sourceId))
                {
                    client.RobotDetails.SourceIds.Add(sourceId);
                }
            }

            if (client.RobotDetails != null)
            {
                client.RobotDetails.LastUpdateTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Attempts to start a conversation for the message stream.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        private async Task<Conversation?> StartConversation(ClientMessage message, IClientConnection client, IExecutionEngine engine, CommandBase command)
        {
            var errors = new List<CommandError>();
            var result = await ValidateAndExecuteCommand(engine, errors, command);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    this.Logger.LogWarning($"Authenticate error: {error.Error}");
                }

                client.SendMessage(GenerateErrorResponse(message, "Session is invalid: cannot start conversation"));
                return null;
            }

            var conversation = result.As<Conversation>().Output!;
            message.ConversationId = conversation.ConversationId;
            return conversation;
        }

        /// <summary>
        /// Starts monitoring all clients.
        /// </summary>
        private Task StartMonitoringAllClients(IExecutionEngine _, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return Task.CompletedTask;

            if (client.Hub == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Client not connected to Hub"));
                return Task.CompletedTask;
            }

            client.Hub.AddMonitor(client);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts program execution on a robot.
        /// </summary>
        private async Task StartProgram(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out IClientConnection? robotClient)) return;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program ID is missing"));
                return;
            }

            if (!int.TryParse(programId, out int programIdValue))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program ID is invalid"));
                return;
            }

            if (!message.Values.TryGetValue("opts", out string? opts))
            {
                opts = "{}";
            }

            logger.LogInformation($"Starting program on {robotClient?.Robot?.MachineName}");
            var clientMessage = GenerateResponse(message, ClientMessageType.StartProgram);
            clientMessage.Values.Add("program", programId);
            clientMessage.Values.Add("opts", opts);
            this.logger.LogInformation($"Starting program {programId} with {opts}");
            robotClient!.SendMessage(clientMessage);
            if (robotClient.Robot != null)
            {
                await AddToRobotLogAsync(engine, robotClient.Robot.MachineName, message, "Program starting");
            }
            else
            {
                this.logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        /// <summary>
        /// Stop monitoring all clients.
        /// </summary>
        private Task StopMonitoringAllClients(IExecutionEngine _, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return Task.CompletedTask;

            if (client.Hub == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Client not connected to Hub"));
                return Task.CompletedTask;
            }

            client.Hub.RemoveClient(client);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends a stop message to the robot.
        /// </summary>
        private async Task StopProgram(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out IClientConnection? robotClient))
            {
                // Cannot retrieve the robot for some reason, tell the client the program has stopped so it can clean up the UI
                client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramStopped));
                return;
            }

            logger.LogInformation($"Stopping program on {robotClient?.Robot?.MachineName}");
            var clientMessage = GenerateResponse(message, ClientMessageType.StopProgram);
            robotClient!.SendMessage(clientMessage);
            if (robotClient.Robot != null)
            {
                await AddToRobotLogAsync(engine, robotClient.Robot.MachineName, message, "Program stopping");
            }
            else
            {
                this.logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        /// <summary>
        /// Triggers the transfer of a program on the robot.
        /// </summary>
        private async Task TransferProgramToRobot(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out IClientConnection? robotClient)) return;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program ID is missing"));
                return;
            }

            if (!int.TryParse(programId, out int programIdValue))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program ID is invalid"));
                return;
            }

            robotClient!.RobotDetails = new RobotStatus
            {
                LastProgramId = programIdValue
            };
            var clientMessage = GenerateResponse(message, ClientMessageType.DownloadProgram);
            clientMessage.Values.Add("program", programId);
            clientMessage.Values.Add("user", client.User!.Name);
            robotClient.SendMessage(clientMessage);
            if (robotClient.Robot != null)
            {
                await AddToRobotLogAsync(engine, robotClient.Robot.MachineName, message, "Program transferring");
            }
            else
            {
                this.logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        /// <summary>
        /// Updates the robot state.
        /// </summary>
        private async Task UpdateRobotState(IExecutionEngine engine, IClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.Robot)) return;

            if (message.Values.TryGetValue("state", out string? state))
            {
                client.Status.IsAvailable = state == "Waiting";
                client.Status.Message = string.IsNullOrWhiteSpace(state) ? "Unknown" : state;
                logger.LogInformation($"Updating state for {client.Robot?.MachineName} to {client.Status.Message}");
            }
            else
            {
                client.Status.Message = "Unknown";
            }

            var msg = GenerateResponse(message, ClientMessageType.RobotStateUpdate);
            foreach (var (key, value) in message.Values)
            {
                msg.Values[key] = value;
            }
            client.NotifyListeners(msg);
            client.LogMessage(msg);
            PopulateSourceValues(client, msg);
            this.hub.SendToMonitors(msg);

            if (client.Robot != null)
            {
                await AddToRobotLogAsync(engine, client.Robot.MachineName, message, $"State updated to {client.Status.Message}");
            }

            if (client.RobotDetails != null)
            {
                client.RobotDetails.LastUpdateTime = DateTime.UtcNow;
            }
        }
    }
}