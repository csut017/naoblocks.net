import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { RobotType } from 'src/app/data/robot-type';
import { RobotTypeService } from 'src/app/services/robot-type.service';

@Component({
  selector: 'app-robot-type-editor',
  templateUrl: './robot-type-editor.component.html',
  styleUrls: ['./robot-type-editor.component.scss']
})
export class RobotTypeEditorComponent implements OnInit, OnChanges {

  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor(private robotTypeService: RobotTypeService) {
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required])
    });
  }
  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      name: this.item?.name || ''
    });
  }

  ngOnInit(): void {
  }

  doSave() {
    if (!this.item) return;
    this.item.name = this.form.get('name')?.value;
    this.robotTypeService.save(this.item)
      .subscribe(result => {
        if (result.successful) {
          this.item!.isNew = false;
          this.closed.emit(true);
        } else {
          this.errors = result.allErrors();
        }
      });
  }

  doClose() {
    this.closed.emit(false);
  }
}
