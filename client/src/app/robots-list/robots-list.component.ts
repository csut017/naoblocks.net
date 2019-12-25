import { Component, OnInit } from '@angular/core';
import { Robot } from '../data/robot';
import { ResultSet } from '../data/result-set';
import { RobotService } from '../services/robot.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-robots-list',
  templateUrl: './robots-list.component.html',
  styleUrls: ['./robots-list.component.scss']
})
export class RobotsListComponent implements OnInit {

  isLoading: boolean = false;
  isInList: boolean = true;
  isInEditor: boolean = false;
  isNew: boolean = false;
  selected: Robot[] = [];
  robots: ResultSet<Robot> = new ResultSet<Robot>();
  currentRobot: Robot;
  message: string;
  errorMessage: string;

  constructor(private robotService: RobotService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = true;
    this.currentRobot = new Robot(true);
  }

  doDelete() {
    forkJoin(this.selected.map(s => this.robotService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        this.message = `Deleted ${successful.length} robots`;
        if (failed.length !== 0) {
          this.errorMessage = `Failed to delete ${successful.length} robots`;
        } else {
          this.errorMessage = undefined;
        }

        this.robots.items = this.robots
            .items
            .filter(el => !successful.includes(el));
        this.robots.count -= successful.length;
      });
  }

  doEdit() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = false;
    this.currentRobot = this.selected[0];
  }

  doExportDetails() {

  }

  doExportLogs() {

  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    this.isInList = true;
    if (saved) {
      if (this.isNew) {
        this.robots.items.push(this.currentRobot);
        this.message = `Added robot '${this.currentRobot.friendlyName}'`;
      } else {
        this.message = `Updated robot '${this.currentRobot.friendlyName}'`;
      }
    }
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
