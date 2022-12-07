import { SelectionModel } from '@angular/cdk/collections';
import { Component, Inject, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatStepper } from '@angular/material/stepper';
import { MatTableDataSource } from '@angular/material/table';
import { Robot } from 'src/app/data/robot';
import { RobotImportSettings } from 'src/app/data/robot-import-settings';
import { RobotType } from 'src/app/data/robot-type';
import { RobotTypeService } from 'src/app/services/robot-type.service';

@Component({
  selector: 'app-robot-type-definition-import',
  templateUrl: './robot-type-definition-import.component.html',
  styleUrls: ['./robot-type-definition-import.component.scss']
})
export class RobotTypeDefinitionImportComponent {
  controlFile: FormControl = new FormControl(null, { validators: Validators.required });
  controlVerification: FormControl = new FormControl(true, { validators: Validators.required });
  columns: string[] = ['select', 'name', 'message'];
  currentIndex: number = 1;
  errorMessage: string = '';
  isFinished: boolean = false;
  isImporting: boolean = false;
  dataSource: MatTableDataSource<RobotType> = new MatTableDataSource();
  results: string[] = [];
  selection = new SelectionModel<RobotType>(true, []);
  successful: number = 0;
  @ViewChild('stepper') stepper?: MatStepper;

  constructor(private robotTypeService: RobotTypeService,
    @Inject(MAT_DIALOG_DATA) public settings: RobotImportSettings) {
  }

  fileBrowseHandler($event: any): void {
    const files = $event.target ? $event.target.files : $event;
    this.robotTypeService.parseImportFile(files[0])
      .subscribe(result => {
        if (!result.successful) {
          this.errorMessage = result.allErrors().join();
          return;
        }

        let robots = result.output?.items || [];
        this.selection.clear();
        this.dataSource = new MatTableDataSource(robots.map(r => {
          if (!r.parse?.message) this.selection.select(r);
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

  checkboxLabel(row?: RobotType): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  startImport(): void {
    this.isImporting = true;
    this.currentIndex = 0;
    this.results = [];
    this.successful = 0;
    this.importRobotType();
  }

  importRobotType(): void {
    let robot = this.selection.selected[this.currentIndex++];
    robot.isNew = true;
    this.robotTypeService.save(robot)
      .subscribe(res => {
        if (res.successful) {
          this.results.push(`Successfully imported ${robot.name}`);
          this.successful++;
        } else {
          this.results.push(`Failed to import ${robot.name}: ${res.allErrors().join(', ')}`);
        }
        if (this.currentIndex < this.selection.selected.length) {
          this.importRobotType();
        } else {
          this.currentIndex = this.selection.selected.length;
          this.isFinished = true;
        }
      });
  }

}
