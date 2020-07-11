import { Component, OnInit, Input } from '@angular/core';
import { Compilation } from '../data/compilation';
import { RoboLangAstToken } from '../data/ast-token';
import { RoboLangAstNode } from '../data/ast-node';
import { DebugMessage } from '../data/debug-message';

@Component({
  selector: 'app-program-display',
  templateUrl: './program-display.component.html',
  styleUrls: ['./program-display.component.scss']
})
export class ProgramDisplayComponent implements OnInit {

  programDetails: Compilation;
  nodes: { [key: string]: RoboLangAstNode } = {};

  loadProgram(value: Compilation) {
    this.programDetails = value;
    this.nodes = {};
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

  updateStatus(msg: DebugMessage): void {
    let node = this.nodes[msg.sourceID];
    if (node) {
      node.statusIcon = msg.status == 'start' ? 'circle-arrow right' : 'success-standard';
    }
  }

  private initialiseNode(node: RoboLangAstNode) {
    if (node.sourceId) {
      this.nodes[node.sourceId] = node;
      node.statusIcon = 'circle';
    } else {
      node.statusIcon = 'ban';
    }
    if (node.arguments) node.arguments.forEach(n => this.initialiseNode(n));
    if (node.children) node.children.forEach(n => this.initialiseNode(n));
  }

}
