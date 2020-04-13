import { Component, OnInit } from '@angular/core';
import { Robot } from '../data/robot';
import { ResultSet } from '../data/result-set';
import { RobotService } from '../services/robot.service';
import { forkJoin } from 'rxjs';
import { FileDownloaderService } from '../services/file-downloader.service';
import { environment } from 'src/environments/environment';

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

  constructor(private robotService: RobotService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew(): void {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = true;
    this.currentRobot = new Robot(true);
  }

  doDelete(): void {
    forkJoin(this.selected.map(s => this.robotService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        if (successful.length !== 0) {
          this.message = `Deleted ${successful.length} robots`;
        } else {
          this.message = undefined;
        }
        if (failed.length !== 0) {
          this.errorMessage = `Failed to delete ${failed.length} robots`;
        } else {
          this.errorMessage = undefined;
        }

        this.robots.items = this.robots
            .items
            .filter(el => !successful.includes(el));
        this.robots.count -= successful.length;
      });
  }

  doEdit(): void {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = false;
    this.currentRobot = this.selected[0];
  }

  doExportList(): void {
    this.downloaderService.download('v1/robots/export/list', 'robots-list.xlsx');
  }

  doExportDetails(): void {

  }

  doExportLogs(): void {

  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    this.isInList = true;
    if (saved) {
      if (this.isNew) {
        this.robots.items.push(this.currentRobot);
        this.message = `Added robot '${this.currentRobot.friendlyName}'`;
        this.currentRobot.id = this.currentRobot.machineName;
      } else {
        this.message = `Updated robot '${this.currentRobot.friendlyName}'`;
      }
    }
  }

  doRefresh(): void {
    this.loadList();
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
