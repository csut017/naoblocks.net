import { Component, OnInit } from '@angular/core';
import { Compilation } from '../data/compilation';
import { RoboLangAstToken } from '../data/ast-token';
import { RoboLangAstNode } from '../data/ast-node';
import { DebugMessage } from '../data/debug-message';
import { HubClient } from '../data/hub-client';
import { ProgramService } from '../services/program.service';

@Component({
  selector: 'app-program-display',
  templateUrl: './program-display.component.html',
  styleUrls: ['./program-display.component.scss']
})
export class ProgramDisplayComponent implements OnInit {

  programDetails: Compilation;
  nodes: { [key: string]: RoboLangAstNode } = {};
  currentClient: HubClient;
  currentProgram: Compilation;
  isLoading: boolean;

  constructor(private programService: ProgramService) { }

  ngOnInit(): void {
  }

  loadProgram(value: Compilation) {
    this.programDetails = value;
    this.nodes = {};
    if (value) value.nodes.forEach(n => this.initialiseNode(n));
  }

  display(client: HubClient) {
    this.currentClient = client;
    this.isLoading = true;
    this.programService.getAST(client.user.name, client.programId.toString())
      .subscribe(result => {
        this.currentProgram = result.output;
        this.loadProgram(this.currentProgram);
        client.messages.forEach(msg => {
          let debug = msg as DebugMessage;
          if (debug && debug.sourceID && (debug.programId == client.programId)) this.updateStatus(debug);
        });
        this.isLoading = false;
      });
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
