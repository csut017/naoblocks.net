import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Robot } from '../data/robot';
import { RobotService } from '../services/robot.service';

@Component({
  selector: 'app-robot-editor',
  templateUrl: './robot-editor.component.html',
  styleUrls: ['./robot-editor.component.scss']
})
export class RobotEditorComponent implements OnInit {

  @Input() robot: Robot;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];
  showPassword: boolean = false;

  constructor(private robotService: RobotService) { }

  ngOnInit() {
  }

  doSave() {
    if (!this.robot.isNew && !this.showPassword) {
      this.robot.password = undefined;
    } else {
      this.robot.password = this.robot.password || '';
    }
    this.robotService.save(this.robot)
      .subscribe(result => {
        if (result.successful) {
          if (result.output) {
            this.robot.whenAdded = result.output.whenAdded;
          }
          this.robot.isNew = false;
          this.closed.emit(true);
        } else {
          this.errors = result.allErrors();
        }
      });
  }

  doCancel() {
    this.closed.emit(false);
  }

}
