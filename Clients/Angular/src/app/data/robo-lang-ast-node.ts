import { RoboLangAstToken } from "./robo-lang-ast-token";

export class RoboLangAstNode {
    sourceId?: string;
    token?: RoboLangAstToken;
    children: RoboLangAstNode[] = [];
    arguments: RoboLangAstNode[] = [];
    type?: string;
  
    statusIcon?: string;
  }
  