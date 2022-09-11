namespace NaoBlocks.Common
{
    /// <summary>
    /// Defines the type of the message.
    /// </summary>
    public enum ClientMessageType
    {
        /// <summary>
        /// The message type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Send the client credentials to the server.
        /// </summary>
        Authenticate = 1,

        /// <summary>
        /// Credentials are valid.
        /// </summary>
        Authenticated = 2,

        /// <summary>
        /// Request a robot to run a program on.
        /// </summary>
        RequestRobot = 11,

        /// <summary>
        /// Allocate a robot to the client.
        /// </summary>
        RobotAllocated = 12,

        /// <summary>
        /// There are no clients available.
        /// </summary>
        NoRobotsAvailable = 13,

        /// <summary>
        /// Request the server to inform the robot to download a program.
        /// </summary>
        TransferProgram = 20,

        /// <summary>
        /// Reply from the server when the robot has finished downloading.
        /// </summary>
        ProgramTransferred = 21,

        /// <summary>
        /// Request the robot to download a program.
        /// </summary>
        DownloadProgram = 22,

        /// <summary>
        /// The robot has finished downloading the program.
        /// </summary>
        ProgramDownloaded = 23,

        /// <summary>
        /// The program cannot be downloaded to the robot.
        /// </summary>
        UnableToDownloadProgram = 24,

        /// <summary>
        /// Start execution of a program.
        /// </summary>
        StartProgram = 101,

        /// <summary>
        /// Program execution has started.
        /// </summary>
        ProgramStarted = 102,

        /// <summary>
        /// Program execution has finished.
        /// </summary>
        ProgramFinished = 103,

        /// <summary>
        /// Request cancellation of a program.
        /// </summary>
        StopProgram = 201,

        /// <summary>
        /// Program has been cancelled.
        /// </summary>
        ProgramStopped = 202,

        /// <summary>
        /// An update from the robot about its state.
        /// </summary>
        RobotStateUpdate = 501,

        /// <summary>
        /// A debug message from the robot (normally a step has started).
        /// </summary>
        RobotDebugMessage = 502,

        /// <summary>
        /// An error that occurred during execution of a program.
        /// </summary>
        RobotError = 503,

        /// <summary>
        /// A general error (e.g. message type not recognised).
        /// </summary>
        Error = 1000,

        /// <summary>
        /// The client has not been authenticated.
        /// </summary>
        NotAuthenticated = 1001,

        /// <summary>
        /// The client is not allowed to call the functionality.
        /// </summary>
        Forbidden = 1002,

        /// <summary>
        /// Start monitoring all client changes.
        /// </summary>
        StartMonitoring = 1100,

        /// <summary>
        /// Stop monitoring all client changes.
        /// </summary>
        StopMonitoring = 1101,

        /// <summary>
        /// A new client has connected to the system.
        /// </summary>
        ClientAdded = 1102,

        /// <summary>
        /// An existing client has disconnected.
        /// </summary>
        ClientRemoved = 1103,

        /// <summary>
        /// Requests all current notifications on the robot.
        /// </summary>
        AlertsRequest = 1200,

        /// <summary>
        /// An alert is being broadcast to all listeners.
        /// </summary>
        AlertBroadcast = 1201,
    }
}