import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { BlockDefinition } from 'src/app/data/block-definition';
import { BlockSet } from 'src/app/data/block-set';
import { RobotType } from 'src/app/data/robot-type';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { UiService } from 'src/app/services/ui.service';

declare var Blockly: any;

@Component({
  selector: 'app-custom-block-editor',
  templateUrl: './custom-block-editor.component.html',
  styleUrls: ['./custom-block-editor.component.scss']
})
export class CustomBlockEditorComponent implements OnInit {

  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  definitions: any[] = [];
  error: string = '';
  form: FormGroup;
  invalidBlocks: any[] = [];
  isValid: boolean = true;
  workspace: any;

  constructor(private uiService: UiService) {
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
    });
    this.uiService.getComponent('angular', 'blocks')
      .subscribe(resp => {
        this.definitions = resp;
        this.initialiseLocalBlocks();
        this.initialiseWorkspace();
      });
  }

  ngOnChanges(_: SimpleChanges): void {
  }

  ngOnInit(): void {
    this.initialiseWorkspace();
  }

  doSave() {
    const definition = Blockly.Xml.workspaceToDom(this.workspace);
    if (!this.item || !this.form.valid) return;
  }

  doClose() {
    this.closed.emit(false);
  }

  private initialiseLocalBlocks(): void {
    let blocks: any[] = [
      {
        "type": "category",
        "message0": "Category %1 Optional %2",
        "args0": [
          {
            "type": "field_input",
            "text": "name",
            "name": "NAME"
          },
          {
            "type": "field_checkbox",
            "name": "OPTIONAL",
            "checked": false
          }
        ],
        "message1": "%1",
        "args1": [
          {
            "type": "input_statement",
            "name": "BLOCKS"
          }
        ],
        "inputsInline": true,
        "previousStatement": "category",
        "nextStatement": "category",
        "colour": 230,
        "tooltip": "Defines a category that contains blocks."
      },
    ];
    this.definitions.forEach(def => {
      blocks.push({
        "type": def.type,
        "message0": def.name,
        "previousStatement": null,
        "nextStatement": null,
        "colour": 120,
        "tooltip": `The ${def.type} block`
      });
    });

    console.groupCollapsed('Initialising block editor blocks');
    try {
      blocks.forEach(function (block) {
        console.log('[NaoLang] Defining ' + block.type);
        Blockly.Blocks[block.type] = {
          init: function () {
            console.log('[NaoLang] Initialising ' + block.type);
            this.jsonInit(block);
            this.setTooltip(function () {
              return block.tooltip;
            });
          }
        };
      });
    } finally {
      console.groupEnd();
    }
  }

  validateWorkspace(event?: any): void {
    if (this.workspace.isDragging()) return;
    var validate = !event ||
      (event.type == Blockly.Events.CREATE) ||
      (event.type == Blockly.Events.MOVE) ||
      (event.type == Blockly.Events.DELETE) ||
      (event.type == Blockly.Events.CHANGE);
    if (!validate) return;

    this.invalidBlocks.forEach(function (block) {
      if (block.rendered) block.setWarningText(null);
    })
    this.invalidBlocks = [];
    this.isValid = true;

    let blocks: any[] = this.workspace.getTopBlocks() || [];
    let names = new Set();
    let blocksToCheck: any[] = [];
    blocks.forEach(block => {
      blocksToCheck.push(block);
      let nextBlock = block.getNextBlock();
      while (nextBlock) {
        blocksToCheck.push(nextBlock);
        nextBlock = nextBlock.getNextBlock();
      }
    });
    blocksToCheck.forEach(block => {
      if (block.type != 'category') {
        this.invalidBlocks.push(block);
        block.setWarningText('This block must be in a category.');
        this.isValid = false;
      } else {
        const name = block.getFieldValue('NAME');
        if (names.has(name)) {
          this.invalidBlocks.push(block);
          block.setWarningText('This category has a duplicated name.');
          this.isValid = false;
        } else {
          names.add(name);
        }
      }

      if (!this.isValid) this.error = 'Some categories or blocks are invalid';
    });

    if (this.isValid) {
      if (blocks.length > 1) {
        this.isValid = false;
        this.error = 'All categories must be connected';
      }
    }
  }

  private initialiseWorkspace(): void {
    console.log(`[BlocklyEditor] Initialising workspace`);
    const categories = [...new Set(this.definitions.map(b => b.category))];
    let toolbox = '<xml><category name="Toolbox" colour="290"><block type="category"></block></category>'
      + categories.map(c => {
        const name = `${c} Blocks`.trim();
        return `<category name="${name}" colour="120">`
          + this.definitions.filter(b => b.category == c).map(b => `<block type="${b.type}"></block>`).join('')
          + '</category>';
      }).join('')
      + '</xml>';

    if (!!this.workspace) {
      this.workspace.dispose();
    }

    Blockly.BlockSvg.START_HAT = true;
    this.workspace = Blockly.inject('blockly', {
      grid: {
        spacing: 20,
        snap: true
      },
      scrollbars: true,
      toolbox: toolbox,
      trashcan: true,
      zoom: {
        controls: true,
        wheel: true
      },
    });

    this.workspace.addChangeListener((evt: any) => this.validateWorkspace(evt));
  }
}
