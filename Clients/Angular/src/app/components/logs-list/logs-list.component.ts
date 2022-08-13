import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Robot } from 'src/app/data/robot';
import { RobotLog } from 'src/app/data/robot-log';
import { RobotLogLine } from 'src/app/data/robot-log-line';
import { RobotLogService } from 'src/app/services/robot-log.service';
import { RobotService } from 'src/app/services/robot.service';

class RobotWrapper {
  constructor(public robot: Robot) {
  }

  hasMore: boolean = false;
  isExpanded: boolean = false;
  isLoaded: boolean = false;
  isLoading: boolean = false;
  lastPage: number = 0;
}

@Component({
  selector: 'app-logs-list',
  templateUrl: './logs-list.component.html',
  styleUrls: ['./logs-list.component.scss']
})
export class LogsListComponent implements OnInit {

  @Output() currentItemChanged = new EventEmitter<string>();

  includeDebug: boolean = false;
  isLoading: boolean = true;
  isLogLoading: boolean = false;
  isLogSelected: boolean = false;
  robots: RobotWrapper[] = [];
  selectedLog: RobotLog = new RobotLog();
  selectedRobot?: Robot;

  constructor(private logService: RobotLogService,
    private robotService: RobotService) { }

  ngOnInit() {
    this.loadList();
  }

  toggleRobot(robot: RobotWrapper): void {
    robot.isExpanded = !robot.isExpanded;
    this.selectedRobot = robot.robot;
    if (robot.isLoaded || this.isLoading) return;
    robot.isLoading = true;
    robot.robot.logs = [];
    this.loadLogsPage(robot, 0);
  }

  loadNextLogsPage(robot: RobotWrapper): void {
    this.loadLogsPage(robot, robot.lastPage + 1);
  }

  loadLogsPage(robot: RobotWrapper, page: number): void {
    this.logService.list(robot.robot.machineName || '', page)
      .subscribe(data => {
        robot.isLoaded = true;
        robot.isLoading = false;
        robot.robot.logs = [
          ...robot.robot.logs,
          ...data.items.map(log => {
            log.typeIcon = this.logMappings[log.type];
            return log;
          })
        ];
        robot.hasMore = robot.robot.logs.length < data.count;
        robot.lastPage = data.page;
      });
  }

  viewLogs(log: RobotLog): void {
    this.selectedLog.selected = false;
    this.selectedLog = log;
    this.selectedLog.selected = true;
    this.isLogSelected = true;
    if (log.lines) return;

    if (!this.selectedRobot) return;

    this.refreshLogs();
  }

  refreshLogs(): void {
    this.isLogLoading = true;
    this.logService.get(this.selectedRobot?.machineName || '', this.selectedLog?.conversationId || 0)
      .subscribe(data => {
        this.selectedLog.lines = data.output?.lines || [];
        this.selectedLog.lines = this.selectedLog.lines
          .map(line => this.initialiseLine(line));
        if (!this.includeDebug) {
          this.selectedLog.lines = this.selectedLog.lines.filter(line => !line.isDebug);
        }
        this.isLogLoading = false;
      });
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotService.list()
      .subscribe(data => {
        this.robots = (data?.items || []).map(r => new RobotWrapper(r));
        this.isLoading = false;
      });
  }

  private logMappings: { [id: number]: string } = {
    0: 'device_unknown',
    1: 'smart_toy',
    2: 'face_6',
    3: 'nest_remote_comfort_sensor',
  };

  private iconMappings: { [id: string]: string } = {
    '1': 'key',
    '2': 'key',
    '11': 'record_voice_over',
    '12': 'record_voice_over',
    '13': 'error',
    '20': 'file_download',
    '21': 'file_download',
    '22': 'file_download',
    '23': 'file_download',
    '24': 'error',
    '101': 'play_arrow',
    '102': 'play_arrow',
    '103': 'check',
    '201': 'stop',
    '202': 'stop',
    '501': 'live_ehlp',
    '502': 'fast_forward',
    '503': 'error',
    '1000': 'error',
    '1001': 'lock',
    '1002': 'lock',
  };

  private initialiseLine(line: RobotLogLine): RobotLogLine {
    if (!line) return line;
    line.icon = this.iconMappings[line.sourceMessageType!.toString()] || 'unknown-status';
    line.isDebug = line.sourceMessageType == 502;
    if (line.isDebug && !!line.values) {
      let funcName = line.values['function'];
      if (funcName) line.description = `Block '${funcName}' [${line.values['status']}]`;
    }
    return line;
  }

}
