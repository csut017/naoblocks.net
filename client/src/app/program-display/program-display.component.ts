import { Component, OnInit, Input } from '@angular/core';
import { Compilation } from '../data/compilation';
import { RoboLangAstToken } from '../data/ast-token';

@Component({
  selector: 'app-program-display',
  templateUrl: './program-display.component.html',
  styleUrls: ['./program-display.component.scss']
})
export class ProgramDisplayComponent implements OnInit {

  @Input() program: Compilation;
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

}
