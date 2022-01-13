import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ConfirmSettings } from 'src/app/data/confirm-settings';
import { EditorSettings } from 'src/app/data/editor-settings';
import { ConfirmService } from 'src/app/services/confirm.service';
import { environment } from 'src/environments/environment';

declare var Blockly: any;

@Component({
  selector: 'app-blockly-editor',
  templateUrl: './blockly-editor.component.html',
  styleUrls: ['./blockly-editor.component.scss']
})
export class BlocklyEditorComponent implements OnInit, OnChanges {

  @Input() editorSettings: EditorSettings = new EditorSettings();
  error: string = '';
  hasChanged: boolean = false;
  invalidBlocks: any[] = [];
  isLoading: boolean = true;
  isValid: boolean = true;
  languageUrl: string = `${environment.apiURL}v1/ui/angular/language`;
  requireEvents: boolean = false;
  workspace: any;

  constructor(
    private confirm: ConfirmService) { }

  ngOnChanges(_: SimpleChanges): void {
    if (this.workspace) {
      let xml = this.buildToolbox();
      this.workspace.updateToolbox(xml);
    }
    if (this.editorSettings) this.isLoading = !this.editorSettings.isLoaded;
  }

  ngOnInit(): void {
    this.initialiseWorkspace();
  }

  validateWorkspace(event?: any): void {
    if (this.workspace.isDragging()) return;
    var validate = !event ||
      (event.type == Blockly.Events.CREATE) ||
      (event.type == Blockly.Events.MOVE) ||
      (event.type == Blockly.Events.DELETE);
    if (!validate) return;

    console.log('[StudentHome] Validating');
    this.hasChanged = true;
    this.error = '';

    this.invalidBlocks.forEach(function (block) {
      if (block.rendered) block.setWarningText(null);
    })
    this.invalidBlocks = [];
    this.isValid = true;
    var blocks = this.workspace.getTopBlocks() || [];

    if (this.requireEvents) {
      blocks.forEach((block: any) => {
        if (!block.type.startsWith('robot_on_')) {
          this.invalidBlocks.push(block);
          block.setWarningText('This cannot be a top level block.');
          this.isValid = false;
        }

        if (!this.isValid) this.error = 'Some top-level blocks are invalid';
      });
    } else {
      if (blocks.length > 1) {
        this.isValid = false;
        this.error = 'All blocks must be connected';
      }
    }
  }

  private buildToolbox(): string {
    let xml = this.editorSettings.toolbox || '<xml><category name="..."></category></xml>';
    this.requireEvents = this.editorSettings.user.events && !this.editorSettings.user.simple;
    return xml;
  }

  private initialiseWorkspace(isReadonly: boolean = false) {
    console.log(`[BlocklyEditor] Initialising workspace`);
    const toolbox = this.buildToolbox();

    let currentBlocks: any;
    if (!!this.workspace) {
      currentBlocks = Blockly.Xml.workspaceToDom(this.workspace);
      this.workspace.dispose();
    }

    Blockly.BlockSvg.START_HAT = true;
    if (!!currentBlocks) {
      Blockly.Xml.domToWorkspace(currentBlocks, this.workspace);
    }

    this.workspace = Blockly.inject('blockly', {
      grid: {
        spacing: 20,
        snap: true
      },
      readOnly: isReadonly,
      scrollbars: true,
      toolbox: toolbox,
      trashcan: true,
      zoom: {
        controls: true,
        wheel: true
      },
    });

    this.configureEditor();
  }

  private configureEditor(): void {
    console.groupCollapsed('Initialising blockly editor');
    try {
      console.log('[StudentHome] Defining colours');
      Blockly.FieldColour.COLOURS = [
        '#f00', '#0f0',
        '#00f', '#ff0',
        '#f0f', '#0ff',
        '#000', '#fff'
      ];
      Blockly.FieldColour.COLUMNS = 2;

      console.log('[StudentHome] Configuring modals');
      let that = this;
      let settings = new ConfirmSettings('');
      Blockly.dialog.setAlert(function (message: string, callback: any) {
        settings.prompt = message;
        settings.showCancel = false;
        that.confirm.confirm(settings)
          .subscribe(_ => callback());
      });
      Blockly.dialog.setConfirm(function (message: string, callback: any) {
        settings.prompt = message;
        settings.showCancel = true;
        that.confirm.confirm(settings)
          .subscribe(result => callback(result));
      });
      // Blockly.dialog.setPrompt(function (message: string, defaultValue: any, callback: any) {
      //   that.userInput.title = message;
      //   that.userInput.action = callback;
      //   that.userInput.showOk = true;
      //   that.userInput.showValue = true;
      //   that.userInput.mode = userInputMode.prompt;
      //   that.userInput.value = defaultValue;
      //   that.userInput.open = true;
      // });

      console.log('[StudentHome] Adding validator');
      this.workspace.addChangeListener((evt: any) => this.validateWorkspace(evt));
    } finally {
      console.groupEnd();
    }
  }
}
