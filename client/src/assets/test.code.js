// These values are copied from ClientMessageType.cs in NaoBlocks.Web
const Authenticate = 1, // Send the client credentials to the server
  Authenticated = 2, // Credentials are valid
  RequestRobot = 11, // Request a robot to run a program on
  RobotAllocated = 12, // Allocate a robot to the client
  NoRobotsAvailable = 13, // There are no clients available
  TransferProgram = 20, // Request the server to inform the robot to download a program
  ProgramTransferred = 21, // Reply from the server when the robot has finished downloading
  DownloadProgram = 22, // Request the robot to download a program
  ProgramDownloaded = 23, // The robot has finished downloading the program
  UnableToDownloadProgram = 24, // The program cannot be downloaded to the robot
  StartProgram = 101, // Start execution of a program
  ProgramStarted = 102, // Program execution has started
  ProgramFinished = 103, // Program execution has finished
  StopProgram = 201, // Request cancellation of a program
  ProgramStopped = 202, // Program has been cancelled
  RobotStateUpdate = 501, // An update from the robot about its state
  RobotDebugMessage = 502, // A debug message from the robot (normally a step has started)
  RobotError = 503, // An error that occurred during execution of a program
  Error = 1000, // A general error (e.g. message type not recognised)
  NotAuthenticated = 1001, // The client has not been authenticated
  Forbidden = 1002; // The client is not allowed to call the functionality

let messages = {};
messages[Authenticated] = "Authenticated";
messages[RobotAllocated] = "Robot Allocated";
messages[NoRobotsAvailable] = "No Robots Available";
messages[ProgramTransferred] = "Program Transferred";
messages[ProgramDownloaded] = "Program Downloaded";
messages[UnableToDownloadProgram] = "Unable To Download Program";
messages[ProgramStarted] = "Program Started";
messages[ProgramFinished] = "Program Finished";
messages[RobotStateUpdate] = "Robot State Update";
messages[RobotDebugMessage] = "Robot Debug Message";
messages[RobotError] = "Robot Error";

class Connection {
  constructor(address) {
    this.address = address;
  }

  compile(code) {
    const url = `http://${this.address}/api/v1/code/compile`;
    console.log(`Compiling code using ${url}`);
    let me = this;
    let request = {
      code: code,
      store: true,
    };

    return $.ajax({
      url: url,
      method: "POST",
      data: JSON.stringify(request),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      headers: {
        Authorization: "Bearer " + (this.session || ""),
      },
    }).done(function (data) {
      // Store the program ID for later, this will be needed when starting a program on the robot
      me.programId = (data.output || {}).programId;
    });
  }

  connect(processor) {
    const url = `ws://${this.address}/api/v1/connections/user`;
    console.log("Starting connection");
    let me = this;
    me.conversationId = undefined;

    me.socket = new WebSocket(url);
    me.socket.onclose = function () {
      console.log("Connection closed");
      me.socket = undefined;
    };
    me.socket.onopen = function () {
      me.send(Authenticate, {
        token: me.session,
      });
    };
    me.socket.onerror = function () {
      console.error("An error has been received");
    };
    me.socket.onmessage = function (msg) {
      const data = JSON.parse(msg.data);
      console.log("Message received");
      console.log(data);
      if (data.conversationId) me.conversationId = data.conversationId;
      processor(data);
    };
  }

  login(username, password) {
    const url = `http://${this.address}/api/v1/session`;
    console.log(`Logging in using ${url}`);
    let me = this;
    let request = {
      name: username,
      password: password,
      role: "Student",
    };

    return $.ajax({
      url: url,
      method: "POST",
      data: JSON.stringify(request),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
    }).done(function (data) {
      console.log(data);
      me.session = data.output.token;
    });
  }

  send(type, data) {
    let msg = {
      type: type,
      values: data,
    };
    if (this.conversationId) msg.conversationId = this.conversationId;
    const jsonData = JSON.stringify(msg);
    this.socket.send(jsonData);
  }
}

const connection = new Connection("localhost:5000");

function logMessage(message) {
  $("#messages").val(function (index, old) {
    return message + "\n" + old;
  });
}

