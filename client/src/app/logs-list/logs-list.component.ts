import { Component, OnInit } from '@angular/core';
import { RobotLog } from '../data/robot-log';
import { ResultSet } from '../data/result-set';
import { RobotLogService } from '../services/robot-log.service';
import { RobotService } from '../services/robot.service';
import { Robot } from '../data/robot';

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

  constructor(private logService: RobotLogService,
    private robotService: RobotService) { }

  ngOnInit() {
    this.loadList();
  }

  loadLogs(robot: Robot): void {
    robot.isLoading = true;
    this.logService.list(robot.machineName)
      .subscribe(data => {
        robot.isLoading = false;
        robot.logs = data.items;
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

}
