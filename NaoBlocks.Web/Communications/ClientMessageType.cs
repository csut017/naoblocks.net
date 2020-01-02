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
        StopProgram = 201,
        ProgramStopped = 202,
        Error = 1000,
    }
}