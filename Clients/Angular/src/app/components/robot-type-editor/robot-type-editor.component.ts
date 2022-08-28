import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { UntypedFormGroup, FormBuilder, Validators, UntypedFormControl } from '@angular/forms';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
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
  form: UntypedFormGroup;
  isSaving: boolean = false;
  toolboxes: Toolbox[] = [];

  constructor(private robotTypeService: RobotTypeService) {
    this.form = new UntypedFormGroup({
      name: new UntypedFormControl('', [Validators.required]),
      type: new UntypedFormControl('', []),
    });
  }
  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      name: this.item?.name || '',
      type: this.item?.isDefault ? 'System default' : 'User selected',
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
