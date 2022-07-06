import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { UiService } from 'src/app/services/ui.service';

declare var Blockly: any;

@Component({
  selector: 'app-toolbox-editor',
  templateUrl: './toolbox-editor.component.html',
  styleUrls: ['./toolbox-editor.component.scss']
})
export class ToolboxEditorComponent implements OnInit {

  @Input() item?: Toolbox;
  @Input() robotType?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  definitions: any[] = [];
  error: string = '';
  form: FormGroup;
  generatorInitialised: boolean = false;
  invalidBlocks: any[] = [];
  isValid: boolean = true;
  workspace: any;

  constructor(private robotTypeService: RobotTypeService,
    private uiService: UiService) {
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
      isDefault: new FormControl(false, [])
    });
    this.uiService.getComponent('angular', 'blocks')
      .subscribe(resp => {
        this.definitions = resp;
        this.initialiseLocalBlocks();
        this.initialiseWorkspace();
      });
  }

  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      name: this.item?.name || '',
      isDefault: this.item?.isDefault || false,
    })
  }

  ngOnInit(): void {
    this.initialiseWorkspace();
    this.initialiseGenerator();
  }

  private initialiseGenerator(): void {
    if (this.generatorInitialised) return;
    Blockly.ToolboxBuilder = new Blockly.Generator('ToolboxBuilder');
    this.generatorInitialised = true;

    Blockly.ToolboxBuilder.finish = function (code: string): string {
      return '<toolbox>' + code + '</toolbox>';
    };

    Blockly.ToolboxBuilder.scrub_ = function (block: any, code: string) {
      let nextBlock = block.nextConnection && block.nextConnection.targetBlock(),
        nextCode = Blockly.ToolboxBuilder.blockToCode(nextBlock);
      return code + nextCode;
    };

    Blockly.ToolboxBuilder.category = function (block: any): string {
      let name = block.getFieldValue('NAME'),
        optional = block.getFieldValue('OPTIONAL'),
        colour = block.getFieldValue('COLOUR'),
        blocks = Blockly.ToolboxBuilder.statementToCode(block, 'BLOCKS'),
        code = `<category name="${name}" colour="${colour}" optional="${optional}">${blocks}</category>`;
      return code;
    }
  }

  doSave() {
    if (!this.item || !this.robotType || !this.form.valid) return;

    console.groupCollapsed('Generating toolbox');
    let definition = '';
    try {
      definition = Blockly.ToolboxBuilder.workspaceToCode(this.workspace);
      console.log(definition);
      console.log((new DOMParser()).parseFromString(definition, "application/xml"));
    } finally {
      console.groupEnd();
    }
    let name = this.form.get('name')?.value || '';
    let isDefault = this.form.get('isDefault')?.value || false;
    this.robotTypeService.importToolbox(this.robotType!, name, definition, isDefault)
      .subscribe(result => {
        if (result.successful) {
          this.closed.emit(true);
        } else {
          this.error = result.allErrors().join(':');
        }
      });
  }

  doClose() {
    this.closed.emit(false);
  }

  private initialiseLocalBlocks(): void {
    this.initialiseGenerator();
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
        "message1": "Colour %1",
        "args1": [
          {
            "type": "field_dropdown",
            "name": "COLOUR",
            "options": [
              ["Red", "330"],
              ["Blue", "210"],
              ["Green", "120"],
              ["Purple", "260"],
              ["Yellow", "65"],
              ["Orange", "20"],
            ]
          }
        ],
        "message2": "Blocks %1",
        "args2": [
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
      Blockly.ToolboxBuilder[def.type] = this.returnCode(def.toolbox);
    });

    console.groupCollapsed('Initialising block editor blocks');
    try {
      blocks.forEach(function (block) {
        console.log('[ToolboxBuilder] Defining ' + block.type);
        Blockly.Blocks[block.type] = {
          init: function () {
            console.log('[ToolboxBuilder] Initialising ' + block.type);
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

  private returnCode(code: string): any {
    return function (block: any): string {
      return code;
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
    console.log(`[ToolboxBuilder] Initialising workspace`);
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
