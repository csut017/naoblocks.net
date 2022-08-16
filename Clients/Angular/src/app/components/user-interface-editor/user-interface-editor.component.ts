import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { UIDefinition } from 'src/app/data/ui-definition';
import { UIDefinitionItem } from 'src/app/data/ui-definition-item';
import { UserSettings } from 'src/app/data/user-settings';
import { UiService } from 'src/app/services/ui.service';

@Component({
  selector: 'app-user-interface-editor',
  templateUrl: './user-interface-editor.component.html',
  styleUrls: ['./user-interface-editor.component.scss']
})
export class UserInterfaceEditorComponent implements OnInit, OnChanges {

  @Input() item?: UIDefinition;
  @Output() closed = new EventEmitter<boolean>();

  description: UIDefinitionItem[] = [];
  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor(private uiService: UiService) {
    this.form = new FormGroup({
      key: new FormControl('', [Validators.required]),
      name: new FormControl('', [Validators.required]),
      description: new FormControl('', []),
    });
   }

  ngOnChanges(_: SimpleChanges): void {
    const key = this.item?.key || '';
    this.form.setValue({
      key: key,
      name: this.item?.name || '',
      description: this.item?.description || '',
    });

    this.uiService.describe(key)
      .subscribe(res => {
        this.description = res.items;
      });
  }

  ngOnInit(): void {
  }

  doSave() {
    if (!this.item) return;
    console.log('TODO: save');
  }

  doClose() {
    this.closed.emit(false);
  }

  getItemCount(item: UIDefinitionItem): string {
    if (item.children?.length) {
      return item.children.length.toString();
    }

    return '';
  }
}
