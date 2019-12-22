import { Component, OnInit } from '@angular/core';
import { Toolbox } from './toolbox';

declare var Blockly: any;

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent implements OnInit {

  workspace: any;

  constructor() { }

  ngOnInit() {
    let xml = new Toolbox()
        .includeConditionals()
        .includeLoops()
        .includeVariables()
        .build();

    this.workspace = Blockly.inject('blocklyDiv', {
      toolbox: xml,
      scrollbars: false
    });
  }
}
