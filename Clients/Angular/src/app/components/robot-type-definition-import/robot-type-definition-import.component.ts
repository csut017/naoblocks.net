import { Component, Inject, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatStepper } from '@angular/material/stepper';
import { forkJoin, Observable } from 'rxjs';
import { ExecutionResult } from 'src/app/data/execution-result';
import { RobotImportSettings } from 'src/app/data/robot-import-settings';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
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
  isDuplicate: boolean = false;
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
        this.isDuplicate = this.definition?.parse?.details?.duplicate || false;
        this.errorMessage = '';
        this.controlFile.setValue(files[0]);
        this.stepper?.next();
      });
  }

  startImport(): void {
    if (!this.definition) return;   // This should not happen...
    this.isImporting = true;
    this.successful = false;
    this.results = [];
    RobotType.setNewStatus(this.definition, !this.isDuplicate);
    this.robotTypeService.save(this.definition)
      .subscribe(r => this.handleBaseImport(r));
  }

  handleBaseImport(res: ExecutionResult<RobotType>) {
    if (!res.successful) {
      this.addResultErrors(res);
      this.isFinished = true;
      return;
    }
    RobotType.setNewStatus(this.definition!, false);
    this.results.push('Imported robot type');
    let updates: any[] = [];
    if (this.definition?.isDefault) {
      updates.push(this.robotTypeService.setSystemDefault(this.definition, 'Set system default'));
    }
    if (!!this.definition?.toolboxes) {
      updates.push(...this.definition!.toolboxes.map(tb => this.importToolbox(tb)));
    }
    if (!!this.definition?.customValues?.length) {
      updates.push(this.robotTypeService.updateAllowedValues(this.definition, this.definition.customValues, 'Import allowed values'));
    }

    forkJoin(updates)
    .subscribe(results => {
      results.forEach(result => {
        let outcome = result.successful ? 'done' : 'failed';
        this.results.push(`${result.message}: ${outcome}`)
        this.addResultErrors(result);
      });
      this.successful = results.reduce((a, cv) => a && cv.successful, true);
      this.isFinished = true;
    });
  }

  private importToolbox(toolbox: Toolbox): Observable<ExecutionResult<Toolbox>> {
    return this.robotTypeService.importToolbox(
      this.definition!,
      toolbox.name!,
      toolbox.definition!,
      toolbox.isDefault || false,
      toolbox.useEvents || false,
      `Import toolbox '${toolbox.name}'`);
  }

  private addResultErrors(result: any) {
    if (result.validationErrors) this.results.push(...result.validationErrors.map(e => `-> ${e}`));
    if (result.executionErrors) this.results.push(...result.executionErrors.map(e => `-> ${e}`));
  }
}
