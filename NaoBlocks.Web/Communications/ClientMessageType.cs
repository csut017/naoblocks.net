namespace NaoBlocks.Web.Communications
{
    public enum ClientMessageType
    {
        Unknown,
        Authenticate = 1,
        Authenticated = 2,
        RequestRobot = 11,
        RobotAllocated = 12,
        NoRobotsAvailable = 13,
        TransferProgram = 20,
        ProgramTransferred = 21,
        StartProgram = 101,
        ProgramStarted = 102,
        ProgramFinished = 103,
        StopProgram = 201,
        ProgramStopped = 202,
        RobotStateUpdate = 501,
        RobotDebugMessage = 502,
        RobotError = 503,
        Error = 1000,
        NotAuthenticated = 1001
    }
}