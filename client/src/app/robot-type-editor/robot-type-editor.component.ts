import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { RobotType } from '../data/robot-type';
import { RobotTypeService } from '../services/robot-type.service';

@Component({
  selector: 'app-robot-type-editor',
  templateUrl: './robot-type-editor.component.html',
  styleUrls: ['./robot-type-editor.component.scss']
})
export class RobotTypeEditorComponent implements OnInit {

  @Input() robotType: RobotType;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];

  constructor(private robotTypeService: RobotTypeService) { }

  ngOnInit() {
  }

  doSave() {
    this.robotTypeService.save(this.robotType)
      .subscribe(result => {
        if (result.successful) {
          this.robotType.isNew = false;
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
