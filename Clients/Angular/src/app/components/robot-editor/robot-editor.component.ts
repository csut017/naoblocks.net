import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Robot } from 'src/app/data/robot';
import { RobotType } from 'src/app/data/robot-type';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-robot-editor',
  templateUrl: './robot-editor.component.html',
  styleUrls: ['./robot-editor.component.scss']
})
export class RobotEditorComponent implements OnInit {

  @Input() item?: Robot;
  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  types: RobotType[] = [];

  constructor(private robotService: RobotService,
    private robotTypeService: RobotTypeService) {
    this.form = new FormGroup({
      friendlyName: new FormControl('', []),
      machineName: new FormControl('', [Validators.required]),
      password: new FormControl('', []),
      type: new FormControl('', [Validators.required]),
    });
  }
  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      machineName: this.item?.machineName || '',
      friendlyName: this.item?.friendlyName || '',
      type: this.item?.type || '',
      password: '',
    });
  }

  ngOnInit(): void {
    this.robotTypeService.list()
      .subscribe(data => this.types = data.items);
  }

  doSave() {
    if (!this.item) return;
    this.item.machineName = this.form.get('machineName')?.value;
    this.item.friendlyName = this.form.get('friendlyName')?.value;
    this.item.type = this.form.get('type')?.value;
    let password = this.form.get('password')?.value || '';
    if (password !== '') this.item.password = password;
    this.robotService.save(this.item)
      .subscribe(result => {
        if (result.successful) {
          this.item!.isNew = false;
          this.item!.friendlyName = result.output?.friendlyName;
          this.item!.whenAdded = result.output?.whenAdded;
          this.item!.isInitialised = result.output?.isInitialised || false;
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
