import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';
import { EditorSettings } from 'src/app/data/editor-settings';
import { PromptSettings } from 'src/app/data/prompt-settings';
import { UserInputMode } from 'src/app/data/user-input-mode';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { SettingsService } from 'src/app/services/settings.service';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  editorSettings: EditorSettings = new EditorSettings();
  hasChanged: boolean = false;
  invalidBlocks: any[] = [];
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );
  isValid: boolean = true;
  onResize: any;
  requireEvents: boolean = true;
  userInput: PromptSettings = new PromptSettings();
  workspace: any;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private settingsService: SettingsService,
    private breakpointObserver: BreakpointObserver) {
    super(authenticationService, router, changeRoleService);
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Student);
    this.settingsService.get()
      .subscribe(s => {
        this.editorSettings = s.output || new EditorSettings();
        let xml = this.buildToolboxXml();
        this.workspace.updateToolbox(xml);
      });

    // this.initialiseWorkspace();
  }

  showSettings(): void {
  }

  // validateWorkspace(event?: any): void {
  //   if (this.workspace.isDragging()) return;
  //   var validate = !event ||
  //     (event.type == Blockly.Events.CREATE) ||
  //     (event.type == Blockly.Events.MOVE) ||
  //     (event.type == Blockly.Events.DELETE);
  //   if (!validate) return;

  //   console.log('[StudentHome] Validating');
  //   this.hasChanged = true;

  //   this.invalidBlocks.forEach(function (block) {
  //     if (block.rendered) block.setWarningText(null);
  //   })
  //   this.invalidBlocks = [];
  //   this.isValid = true;
  //   var blocks = this.workspace.getTopBlocks();

  //   if (this.requireEvents) {
  //     blocks.forEach((block: any) => {
  //       if (!block.type.startsWith('robot_on_')) {
  //         this.invalidBlocks.push(block);
  //         block.setWarningText('This cannot be a top level block.');
  //         this.isValid = false;
  //       }
  //     });
  //   }
  // }

  private buildToolboxXml(): string {
    let xml = this.editorSettings.toolbox || '<xml><category name="Loading..."></category></xml>';
    this.requireEvents = this.editorSettings.user.events && !this.editorSettings.user.simple;
    return xml;
  }

  // private configureEditor(): void {
  //   console.groupCollapsed('Initialising blockly editor');
  //   try {
  //     console.log('[StudentHome] Defining colours');
  //     Blockly.FieldColour.COLOURS = [
  //       '#f00', '#0f0',
  //       '#00f', '#ff0',
  //       '#f0f', '#0ff',
  //       '#000', '#fff'
  //     ];
  //     Blockly.FieldColour.COLUMNS = 2;

  //     console.log('[StudentHome] Configuring modals');
  //     let that = this;
  //     Blockly.alert = function (message: string, callback: any) {
  //       that.userInput.title = message;
  //       that.userInput.action = callback;
  //       that.userInput.showOk = false;
  //       that.userInput.showValue = false;
  //       that.userInput.mode = UserInputMode.alert;
  //       that.userInput.open = true;
  //     };
  //     Blockly.confirm = function (message: string, callback: any) {
  //       that.userInput.title = message;
  //       that.userInput.action = callback;
  //       that.userInput.showOk = true;
  //       that.userInput.showValue = false;
  //       that.userInput.mode = UserInputMode.alert;
  //       that.userInput.open = true;
  //     };
  //     Blockly.prompt = function (message: string, defaultValue: any, callback: any) {
  //       that.userInput.title = message;
  //       that.userInput.action = callback;
  //       that.userInput.showOk = true;
  //       that.userInput.showValue = true;
  //       that.userInput.mode = UserInputMode.prompt;
  //       that.userInput.value = defaultValue;
  //       that.userInput.open = true;
  //     };

  //     console.log('[StudentHome] Adding validator');
  //     this.workspace.addChangeListener((evt: any) => this.validateWorkspace(evt));
  //   } finally {
  //     console.groupEnd();
  //   }
  // }

  // private initialiseWorkspace(isReadonly: boolean = false) {
  //   console.log(`[StudentHome] Initialising workspace`);
  //   let xml = this.buildToolboxXml();
  //   let currentBlocks: any;
  //   if (!!this.workspace) {
  //     currentBlocks = Blockly.Xml.workspaceToDom(this.workspace);
  //     this.workspace.dispose();
  //   }

  //   Blockly.BlockSvg.START_HAT = true;
  //   let blocklyArea = document.getElementById('blocklyArea')!;
  //   let blocklyDiv = document.getElementById('blocklyDiv')!;
  //   this.workspace = Blockly.inject('blocklyDiv', {
  //     readOnly: isReadonly,
  //     toolbox: xml,
  //     grid: {
  //       spacing: 20,
  //       snap: true
  //     },
  //     zoom: {
  //       controls: true,
  //       wheel: true
  //     }
  //   });
  //   if (!!currentBlocks) {
  //     Blockly.Xml.domToWorkspace(currentBlocks, this.workspace);
  //   }

  //   this.configureEditor();

  //   // Initialise the resizing
  //   const workspace = this.workspace;
  //   this.onResize = function () {
  //     let element: any = blocklyArea;
  //     var x = 0;
  //     var y = 0;
  //     do {
  //       x += element.offsetLeft;
  //       y += element.offsetTop;
  //       element = element.offsetParent;
  //     } while (element);
  //     blocklyDiv.style.left = x + 'px';
  //     blocklyDiv.style.top = y + 'px';
  //     blocklyDiv.style.width = blocklyArea.offsetWidth + 'px';
  //     blocklyDiv.style.height = blocklyArea.offsetHeight + 'px';
  //     Blockly.svgResize(workspace);
  //   };
  //   window.addEventListener('resize', _ => this.onResize(), false);
  //   setInterval(() => this.onResize(), 0);
  // }
}
