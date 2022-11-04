import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatStepper } from '@angular/material/stepper';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-robot-import-dialog',
  templateUrl: './robot-import-dialog.component.html',
  styleUrls: ['./robot-import-dialog.component.scss']
})
export class RobotImportDialogComponent implements OnInit {

  controlFile: FormControl = new FormControl(null, { validators: Validators.required });
  controlVerification: FormControl = new FormControl(null, { validators: Validators.required });
  isFinished: boolean = false;
  @ViewChild('stepper') stepper?: MatStepper;

  constructor(private robotService: RobotService) { }

  ngOnInit(): void {
  }

  fileBrowseHandler($event: any): void {
    const files = $event.target ? $event.target.files : $event;
    this.robotService.parseImportFile(files[0])
      .subscribe(result => {
        if (result.errorMsg) {
          return;
        }
        this.controlFile.setValue(files[0]);
        this.stepper?.next();
      });
  }
}
