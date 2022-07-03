import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RobotType } from 'src/app/data/robot-type';

@Component({
  selector: 'app-custom-block-editor',
  templateUrl: './custom-block-editor.component.html',
  styleUrls: ['./custom-block-editor.component.scss']
})
export class CustomBlockEditorComponent implements OnInit {

  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor() {
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
      type: new FormControl('', []),
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
  }

  doClose() {
    this.closed.emit(false);
  }
}
