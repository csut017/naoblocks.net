namespace NaoBlocks.Web.Communications
{
    public enum ClientMessageType
    {
        Unknown,
        Authenticate = 1,           // Send the client credentials to the server
        Authenticated = 2,          // Credentials are valid
        RequestRobot = 11,          // Request a robot to run a program on
        RobotAllocated = 12,        // Allocate a robot to the client
        NoRobotsAvailable = 13,     // There are no clients available
        TransferProgram = 20,       // Request the server to inform the robot to download a program
        ProgramTransferred = 21,    // Reply from the server when the robot has finished downloading
        DownloadProgram = 22,       // Request the robot to download a program
        ProgramDownloaded = 23,     // The robot has finished downloading the program
        StartProgram = 101,         // Start execution of a program
        ProgramStarted = 102,       // Program execution has started
        ProgramFinished = 103,      // Program execution has finished
        StopProgram = 201,          // Request cancellation of a program
        ProgramStopped = 202,       // Program has been cancelled
        RobotStateUpdate = 501,     // An update from the robot about its state
        RobotDebugMessage = 502,    // A debug message from the robot (normally a step has started)
        RobotError = 503,           // An error that occurred during execution of a program
        Error = 1000,               // A general error (e.g. message type not recognised)
        NotAuthenticated = 1001     // The client has not been authenticated
    }
}