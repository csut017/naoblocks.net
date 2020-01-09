import { Component, OnInit } from '@angular/core';
import { RobotLog } from '../data/robot-log';
import { ResultSet } from '../data/result-set';
import { RobotLogService } from '../services/robot-log.service';
import { RobotService } from '../services/robot.service';
import { Robot } from '../data/robot';
import { RobotLogLine } from '../data/robot-log-line';

@Component({
  selector: 'app-logs-list',
  templateUrl: './logs-list.component.html',
  styleUrls: ['./logs-list.component.scss']
})
export class LogsListComponent implements OnInit {

  isLoading: boolean = false;
  isInList: boolean = true;
  robots: ResultSet<Robot> = new ResultSet<Robot>();
  currentRobot: RobotLog;
  message: string;
  errorMessage: string;
  selectedRobot: Robot;
  selectedLog: RobotLog = new RobotLog();
  isLogSelected: boolean;
  isLogLoading: boolean;

  constructor(private logService: RobotLogService,
    private robotService: RobotService) { }

  ngOnInit() {
    this.loadList();
  }

  loadLogs(robot: Robot): void {
    robot.isLoading = true;
    this.selectedRobot = robot;
    this.logService.list(robot.machineName)
      .subscribe(data => {
        robot.isLoading = false;
        robot.logs = data.items;
      });
  }

  viewLogs(log: RobotLog): void {
    this.selectedLog.selected = false;
    this.selectedLog = log;
    this.selectedLog.selected = true;
    this.isLogSelected = true;
    if (log.lines) {
      return;
    }

    this.isLogLoading = true;
    this.logService.get(this.selectedRobot.machineName, this.selectedLog.conversationId)
      .subscribe(data => {
        this.selectedLog.lines = data.output.lines;
        this.selectedLog.lines = this.selectedLog.lines
          .map(line => this.initialiseLine(line))
          .filter(line => !line.skip);
        this.isLogLoading = false;
      });
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotService.list()
      .subscribe(data => {
        this.robots = data;
        this.isLoading = false;
      });
  }

  private iconMappings: { [id: string]: string } = {
    'Authenticate': 'unlock',
    'Authenticated': 'unlock',
    'RequestRobot': 'assign-user',
    'RobotAllocated': 'assign-user',
    'NoRobotsAvailable': 'error-standard',
    'TransferProgram': 'download',
    'ProgramTransferred': 'download',
    'DownloadProgram': 'download',
    'ProgramDownloaded': 'download',
    'UnableToDownloadProgram': 'error-standard',
    'StartProgram': 'play',
    'ProgramStarted': 'play',
    'ProgramFinished': 'check',
    'StopProgram': 'stop',
    'ProgramStopped': 'stop',
    'RobotStateUpdate': 'help-info',
    'RobotDebugMessage': 'step-forward',
    'RobotError': 'error-standard',
    'Error': 'error-standard',
    'NotAuthenticated': 'lock',
    'Forbidden': 'lock',
  };

  private initialiseLine(line: RobotLogLine): RobotLogLine {
    line.icon = this.iconMappings[line.sourceMessageType] || 'unknown-status';
    if (line.values) {
      let funcName = line.values.function;
      if (funcName) line.description += ` [${funcName}-${line.values.status}]`;
    }
    line.skip = line.sourceMessageType == 'RobotStateUpdate';
    return line;
  }

}
