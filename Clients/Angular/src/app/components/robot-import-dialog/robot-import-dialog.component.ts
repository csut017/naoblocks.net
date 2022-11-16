import { SelectionModel } from '@angular/cdk/collections';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatStepper } from '@angular/material/stepper';
import { MatTableDataSource } from '@angular/material/table';
import { Robot } from 'src/app/data/robot';
import { RobotImportSettings } from 'src/app/data/robot-import-settings';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-robot-import-dialog',
  templateUrl: './robot-import-dialog.component.html',
  styleUrls: ['./robot-import-dialog.component.scss']
})
export class RobotImportDialogComponent implements OnInit {

  controlFile: FormControl = new FormControl(null, { validators: Validators.required });
  controlVerification: FormControl = new FormControl(true, { validators: Validators.required });
  columns: string[] = ['select', 'machineName', 'friendlyName', 'type', 'password', 'message'];
  currentIndex: number = 1;
  errorMessage: string = '';
  isFinished: boolean = false;
  isImporting: boolean = false;
  dataSource: MatTableDataSource<Robot> = new MatTableDataSource();
  results: string[] = [];
  selection = new SelectionModel<Robot>(true, []);
  successful: number = 0;
  @ViewChild('stepper') stepper?: MatStepper;

  constructor(private robotService: RobotService,
    @Inject(MAT_DIALOG_DATA) public settings: RobotImportSettings) {
  }

  ngOnInit(): void {
  }

  fileBrowseHandler($event: any): void {
    const files = $event.target ? $event.target.files : $event;
    this.robotService.parseImportFile(files[0])
      .subscribe(result => {
        if (!result.successful) {
          this.errorMessage = result.allErrors().join();
          return;
        }

        let robots = result.output?.items || [];
        this.selection.clear();
        this.dataSource = new MatTableDataSource(robots.map(r => {
          if (!r.message) this.selection.select(r);
          if (this.settings.robotType) {
            r.type = this.settings.robotType;
          }
          return r;
        }));

        this.errorMessage = '';
        this.controlFile.setValue(files[0]);
        this.stepper?.next();
      });
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.selection.select(...this.dataSource.data);
  }

  checkboxLabel(row?: Robot): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.machineName}`;
  }

  startImport(): void {
    this.isImporting = true;
    this.currentIndex = 0;
    this.results = [];
    this.successful = 0;
    this.importRobot();
  }

  importRobot(): void {
    let robot = this.selection.selected[this.currentIndex++];
    robot.isNew = true;
    this.robotService.save(robot)
      .subscribe(res => {
        if (res.successful) {
          this.results.push(`Successfully imported ${robot.machineName}`);
          this.successful++;
        } else {
          this.results.push(`Failed to import ${robot.machineName}: ${res.allErrors().join(', ')}`);
        }
        if (this.currentIndex < this.selection.selected.length) {
          this.importRobot();
        } else {
          this.currentIndex = this.selection.selected.length;
          this.isFinished = true;
        }
      });
  }
}
