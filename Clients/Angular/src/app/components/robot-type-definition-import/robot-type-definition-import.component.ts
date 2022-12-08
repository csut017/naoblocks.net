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
  definition?: RobotType;
  errorMessage: string = '';
  isFinished: boolean = false;
  isImporting: boolean = false;
  results: string[] = [];
  successful: boolean = true;
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

        this.definition = result.output;
        this.errorMessage = '';
        this.controlFile.setValue(files[0]);
        this.stepper?.next();
      });
  }

  startImport(): void {
    this.isImporting = true;
    this.successful = false;
    this.results = [];
    // TODO: start import
  }

}
