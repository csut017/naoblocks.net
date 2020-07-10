import { Component, OnInit, Input } from '@angular/core';
import { Compilation } from '../data/compilation';
import { RoboLangAstToken } from '../data/ast-token';
import { RoboLangAstNode } from '../data/ast-node';

@Component({
  selector: 'app-program-display',
  templateUrl: './program-display.component.html',
  styleUrls: ['./program-display.component.scss']
})
export class ProgramDisplayComponent implements OnInit {

  programDetails: Compilation;

  @Input() set program(value: Compilation){
    this.programDetails = value;
    if (value) value.nodes.forEach(n => this.initialiseNode(n));
  }

  @Input() isLoading: boolean;

  constructor() { }

  ngOnInit(): void {
  }

  formatToken(token: RoboLangAstToken): string {
    switch (token.type) {
      case 'Text':
        return `'${token.value}'`;

      case 'Colour':
        return `#${token.value}`;
      
      default:
        return token.value;
    }
  }

  private initialiseNode(node: RoboLangAstNode) {
    node.statusIcon = 'circle';
    if (node.arguments) node.arguments.forEach(n => this.initialiseNode(n));
    if (node.children) node.children.forEach(n => this.initialiseNode(n));
  }

}
