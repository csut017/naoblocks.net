using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IHub _hub;
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors;
        private readonly IDocumentStore _store;

        public MessageProcessor(IHub hub, ILogger<MessageProcessor> logger, IDocumentStore store)
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
            this._hub = hub;
            this._logger = logger;
            this._store = store;
        }

        private async Task ProgramDownloaded(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            var msg = GenerateResponse(message, ClientMessageType.ProgramTransferred);
            msg.Values["ProgramId"] = client?.RobotDetails?.LastProgramId?.ToString(CultureInfo.InvariantCulture) ?? "0";
            client.NotifyListeners(msg);
            client.LogMessage(msg);
            PopulateSourceValues(client, msg);
            client.Hub?.SendToMonitors(msg);
            if ((client != null) && (client.Robot != null))
            {
                await AddToRobotLogAsync(session, client.Robot.Id, message, "Program transferred");
            }
        }

        private delegate Task TypeProcessor(IAsyncDocumentSession session, ClientConnection client, ClientMessage message);

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Message processor should catch any errors and pass it to the client")]
        public async Task ProcessAsync(ClientConnection client, ClientMessage message)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (this._processors.TryGetValue(message.Type, out TypeProcessor? processor) && (processor != null))
            {
                try
                {
                    using var session = this._store.OpenAsyncSession();
                    this._logger.LogInformation($"Processing message type {message.Type}");
                    await processor(session, client, message);
                    await session.SaveChangesAsync();
                }
                catch (Exception err)
                {
                    this._logger.LogWarning($"An error occurred while processing {message.Type}: {err.Message}");
                    client.SendMessage(GenerateErrorResponse(message, $"Unable to process message: {err.Message}"));
                }
            }
            else
            {
                this._logger.LogWarning($"Unable to find processor for message type {message.Type}");
                client.SendMessage(GenerateErrorResponse(message, $"Unable to find processor for {message.Type}"));
            }
        }

        private static async Task AddToRobotLogAsync(IAsyncDocumentSession session, string robotId, ClientMessage message, string description, bool skipValues = false)
        {
            var line = new RobotLogLine
            {
                Description = description,
                SourceMessageType = message.Type,
                WhenAdded = DateTime.UtcNow
            };
            if (!skipValues)
            {
                foreach (var (key, value) in message.Values)
                {
                    line.Values.Add(new RobotLogLineValue
                    {
                        Name = key,
                        Value = value
                    });
                }
            }

            var log = await GetOrAddRobotLogAsync(session, robotId, message.ConversationId ?? 0);
            log.Lines.Add(line);
            log.WhenLastUpdated = DateTime.UtcNow;
        }

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

        private static ClientMessage GenerateResponse(ClientMessage request, ClientMessageType type)
        {
            var response = new ClientMessage
            {
                Type = type,
                ConversationId = request.ConversationId
            };
            return response;
        }

        private static async Task<RobotLog> GetOrAddRobotLogAsync(IAsyncDocumentSession session, string robotId, long conversationId)
        {
            var log = await session.Query<RobotLog>()
                .FirstOrDefaultAsync(rl => rl.RobotId == robotId && rl.Conversation.ConversationId == conversationId);
            var conversation = await session.Query<Conversation>()
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
            if (log == null)
            {
                log = new RobotLog
                {
                    Conversation = conversation,
                    RobotId = robotId,
                    WhenAdded = DateTime.UtcNow,
                    WhenLastUpdated = DateTime.UtcNow
                };
                await session.StoreAsync(log);
            }
            return log;
        }

        private static bool ValidateRequest(ClientConnection client, ClientMessage message, ClientConnectionType? requiredRole = null)
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

        private async Task AllocateRobot(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;

            ClientConnection? nextRobot = null;
            if ((client.User != null) && (client.User.Settings != null) && (client.User.Settings.AllocationMode > 0))
            {
                nextRobot = this._hub.GetClients(ClientConnectionType.Robot)
                    .FirstOrDefault(r => (r.Robot != null) && (r.Robot.MachineName == client.User.Settings.RobotId) && r.Status.IsAvailable);
                if ((nextRobot == null) && (client.User.Settings.AllocationMode == 1))
                {
                    this._logger.LogInformation($"No robots available for allocation");
                    client.SendMessage(GenerateResponse(message, ClientMessageType.NoRobotsAvailable));
                    return;
                }
            }

            if (nextRobot == null)
            {
                var rnd = new Random();
                nextRobot = this._hub.GetClients(ClientConnectionType.Robot)
                    .OrderBy(r => r.Status.LastAllocatedTime)
                    .ThenBy(_ => rnd.Next())
                    .FirstOrDefault(r => r.Status.IsAvailable);
            }

            if (nextRobot == null)
            {
                this._logger.LogInformation($"No robots available for allocation");
                client.SendMessage(GenerateResponse(message, ClientMessageType.NoRobotsAvailable));
                return;
            }

            nextRobot.Status.IsAvailable = false;
            nextRobot.Status.LastAllocatedTime = DateTime.UtcNow;
            nextRobot.AddListener(client);
            var response = GenerateResponse(message, ClientMessageType.RobotAllocated);
            response.Values["robot"] = nextRobot.Id.ToString(CultureInfo.InvariantCulture);
            client.SendMessage(response);

            client.LogMessage(response);
            PopulateSourceValues(client, response);
            client?.Hub.SendToMonitors(response);

            this._logger.LogInformation($"Allocated robot {nextRobot.Id}");
            await AddToRobotLogAsync(session, nextRobot.Robot.Id, message, "Robot allocated to user");
        }

        private async Task Authenticate(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (!message.Values.TryGetValue("token", out string? token))
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is missing"));
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var sessionId = jwtToken.Claims.FirstOrDefault(c => c.Type == "SessionId");
            if (sessionId == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is invalid"));
                return;
            }

            var userSession = await session.LoadAsync<Session>(sessionId.Value);
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

            if (userSession.IsRobot)
            {
                client.Robot = await session.LoadAsync<Robot>(userSession.UserId);
                if (client?.Robot != null)
                {
                    client.Robot.Type = await session.LoadAsync<RobotType>(client.Robot.RobotTypeId);
                    await AddToRobotLogAsync(session, client.Robot.Id, message, "Robot authenticated", false);
                }

                msg.Values["Type"] = "robot";
                msg.Values["SubType"] = client?.Robot?.Type?.Name ?? "Unknown";
                msg.Values["Name"] = client?.Robot?.FriendlyName ?? "Unknown";
            }
            else
            {
                client.User = await session.LoadAsync<User>(userSession.UserId);
                var systemValues = await session.Query<SystemValues>().FirstOrDefaultAsync();
                if (systemValues == null)
                {
                    systemValues = new SystemValues();
                    await session.StoreAsync(systemValues);
                }
                var conversationId = ++systemValues.NextConversationId;
                message.ConversationId = conversationId;
                await session.StoreAsync(new Conversation
                {
                    ConversationId = conversationId,
                    UserId = client.User.Id,
                    UserName = client.User.Name
                });

                msg.Values["Type"] = "user";
                msg.Values["SubType"] = client?.User?.Role.ToString() ?? "Unknown";
                msg.Values["Name"] = client?.User?.Name ?? "Unknown";
                msg.Values["IsStudent"] = (client?.User?.Role ?? UserRole.Student) == UserRole.Student ? "yes" : "no";
            }

            client.LogMessage(msg);
            client.Hub?.SendToMonitors(msg);
            client.SendMessage(GenerateResponse(message, ClientMessageType.Authenticated));
        }

        private TypeProcessor BroadcastMessage(ClientMessageType messageType, string logDescription, bool includeValues = false)
        {
            return async (IAsyncDocumentSession session, ClientConnection client, ClientMessage message) =>
            {
                await this.DoBroadcastMessage(session, client, message, messageType, logDescription, includeValues);
            };
        }

        private async Task DoBroadcastMessage(IAsyncDocumentSession session, ClientConnection client, ClientMessage message, ClientMessageType messageType, string logDescription, bool includeValues)
        {
            var msg = GenerateResponse(message, messageType);
            if (includeValues)
            {
                foreach (var (key, value) in message.Values)
                {
                    msg.Values[key] = value;
                }
            }
            client.NotifyListeners(msg);
            client.LogMessage(msg);
            PopulateSourceValues(client, msg);
            client.Hub?.SendToMonitors(msg);
            if ((client != null) && (client.Robot != null))
            {
                await AddToRobotLogAsync(session, client.Robot.Id, message, logDescription);
            }
        }

        private bool RetrieveRobot(ClientConnection client, ClientMessage message, out ClientConnection? robotClient)
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

            robotClient = this._hub.GetClient(robotId);
            if (robotClient == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot is not longer connected"));
                return false;
            }

            return true;
        }

        private async Task StartProgram(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient)) ;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program is missing"));
                return;
            }

            if (!message.Values.TryGetValue("opts", out string? opts))
            {
                opts = "{}";
            }

            var clientMessage = GenerateResponse(message, ClientMessageType.StartProgram);
            clientMessage.Values.Add("program", programId);
            clientMessage.Values.Add("opts", opts);
            this._logger.LogInformation($"Starting program {programId} with {opts}");
            robotClient?.SendMessage(clientMessage);
            if ((robotClient != null) && (robotClient.Robot != null))
            {
                await AddToRobotLogAsync(session, robotClient.Robot.Id, message, "Program starting");
            }
            else
            {
                this._logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        private async Task StopProgram(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient))
            {
                // Cannot retrieve the robot for some reason, tell the client the program has stopped so it can clean up the UI
                client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramStopped));
                return;
            }

            var clientMessage = GenerateResponse(message, ClientMessageType.StopProgram);
            robotClient?.SendMessage(clientMessage);
            if ((robotClient != null) && (robotClient.Robot != null))
            {
                await AddToRobotLogAsync(session, robotClient.Robot.Id, message, "Program stopping");
            }
            else
            {
                this._logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        private async Task TransferProgramToRobot(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient) || (robotClient == null)) return;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program is missing"));
                return;
            }

            if (!int.TryParse(programId, out int programIdValue)) {
                client.SendMessage(GenerateErrorResponse(message, "Invalid program ID"));
                return;
            }

            robotClient.RobotDetails = new RobotStatus
            {
                LastProgramId = programIdValue
            };
            var clientMessage = GenerateResponse(message, ClientMessageType.DownloadProgram);
            clientMessage.Values.Add("program", programId);
            clientMessage.Values.Add("user", client.User?.Name ?? string.Empty);
            robotClient?.SendMessage(clientMessage);
            if ((robotClient != null) && (robotClient.Robot != null))
            {
                await AddToRobotLogAsync(session, robotClient.Robot.Id, message, "Program transferring");
            }
            else
            {
                this._logger.LogWarning("Unable to add to log: robot is missing");
            }
        }

        private async Task UpdateRobotState(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.Robot)) return;

            if (message.Values.TryGetValue("state", out string? state))
            {
                client.Status.IsAvailable = state == "Waiting";
                client.Status.Message = state ?? "Unknown";
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
            client.Hub?.SendToMonitors(msg);

            if (client.Robot != null)
            {
                state ??= "Unknown";
                await AddToRobotLogAsync(session, client.Robot.Id, message, $"State updated to {state}");
            }
        }

        private Task HandleAlertsRequest(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.User)) return Task.CompletedTask;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient) || (robotClient == null)) return Task.CompletedTask;

            foreach (var alert in robotClient.Notifications)
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

        private Task HandleBroadcastAlert(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message, ClientConnectionType.Robot)) return Task.CompletedTask;

            var alert = new NotificationAlert
            {
                Id = int.TryParse(message.Values.TryGetValue("id", out string? value) ? value : "-1", out int idValue) ? idValue : -1,
                Message = message.Values.TryGetValue("message", out string? msg) ? msg : string.Empty,
                Severity = message.Values.TryGetValue("severity", out string? severity) ? severity : "info",
                WhenAdded = DateTime.UtcNow
            };

            client.Notifications.Add(alert);
            if (client.Notifications.Count > 20) client.Notifications.RemoveAt(0);

            var broadcast = new ClientMessage(ClientMessageType.AlertBroadcast, new {
                id = alert.Id,
                message = alert.Message,
                severity = alert.Severity
            });
            PopulateSourceValues(client, broadcast);
            client.Hub?.SendToMonitors(broadcast);
            return Task.CompletedTask;
        }

        private static void PopulateSourceValues(ClientConnection client, ClientMessage msg)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            msg.Values["SourceId"] = client.Id.ToString(CultureInfo.InvariantCulture);
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

        private async Task RobotDebugMessage(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            await this.DoBroadcastMessage(session, client, message, ClientMessageType.RobotDebugMessage, "Debug information received", true);
            if (client.RobotDetails != null)
            {
                if (message.Values.TryGetValue("sourceID", out string? sourceId) && !string.IsNullOrEmpty(sourceId))
                {
                    client.RobotDetails.SourceIds.Add(sourceId);
                }
            }
        }

        private Task StartMonitoringAllClients(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.Hub == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Client not connected to Hub"));
                return Task.CompletedTask;
            }

            client.Hub.AddMonitor(client);
            return Task.CompletedTask;
        }

        private Task StopMonitoringAllClients(IAsyncDocumentSession session, ClientConnection client, ClientMessage message)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (client.Hub == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Client not connected to Hub"));
                return Task.CompletedTask;
            }

            client.Hub.RemoveMonitor(client);
            return Task.CompletedTask;
        }
    }
}