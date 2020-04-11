import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Robot } from '../data/robot';
import { RobotService } from '../services/robot.service';
import { RobotType } from '../data/robot-type';
import { RobotTypeService } from '../services/robot-type.service';

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
  types: RobotType[] = [];

  constructor(private robotService: RobotService,
    private robotTypeService: RobotTypeService) { }

  ngOnInit() {
    this.robotTypeService.list()
      .subscribe(results => {
        this.types = results.items;
      });
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
