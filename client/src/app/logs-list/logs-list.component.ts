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
    '1': 'unlock',
    '2': 'unlock',
    '11': 'assign-user',
    '12': 'assign-user',
    '13': 'error-standard',
    '20': 'download',
    '21': 'download',
    '22': 'download',
    '23': 'download',
    '24': 'error-standard',
    '101': 'play',
    '102': 'play',
    '103': 'check',
    '201': 'stop',
    '202': 'stop',
    '501': 'help-info',
    '502': 'step-forward',
    '503': 'error-standard',
    '1000': 'error-standard',
    '1001': 'lock',
    '1002': 'lock',
  };

  private initialiseLine(line: RobotLogLine): RobotLogLine {
    line.icon = this.iconMappings[line.sourceMessageType.toString()] || 'unknown-status';
    if (line.values) {
      let funcName = line.values.function;
      if (funcName) line.description += ` [${funcName}-${line.values.status}]`;
    }
    line.skip = line.sourceMessageType == 502;
    return line;
  }

}
