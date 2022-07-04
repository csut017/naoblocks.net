import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { BlockDefinition } from 'src/app/data/block-definition';
import { BlockSet } from 'src/app/data/block-set';
import { RobotType } from 'src/app/data/robot-type';
import { RobotTypeService } from 'src/app/services/robot-type.service';

@Component({
  selector: 'app-custom-block-editor',
  templateUrl: './custom-block-editor.component.html',
  styleUrls: ['./custom-block-editor.component.scss']
})
export class CustomBlockEditorComponent implements OnInit {

  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  blocks: BlockDefinition[] = [];
  blockSets: BlockSet[] = [];
  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  availableBlocks: BlockDefinition[] = [];
  usedBlocks: BlockDefinition[] = [];

  constructor(private robotTypeService: RobotTypeService) {
    this.form = new FormGroup({
      setName: new FormControl('', [Validators.required]),
      name: new FormControl('', []),
    });
  }
  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      setName: '<new>',
      name: '',
    });
    if (!!this.item) {
      this.robotTypeService.listBlockSets(this.item?.id || '', true)
        .subscribe(res => {
          this.blockSets = res.blockSets || [];
          this.blocks = res.blocks || [];
          this.availableBlocks = this.blocks;
          this.availableBlocks.sort((a, b) => a.name!.localeCompare(b.name!));
        });
    }
  }

  ngOnInit(): void {
  }

  doSave() {
    if (!this.item) return;
  }

  doClose() {
    this.closed.emit(false);
  }

  drop(event: CdkDragDrop<BlockDefinition[]>) {
    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex,
    );
    this.availableBlocks.sort((a, b) => a.name!.localeCompare(b.name!));
  }
}
