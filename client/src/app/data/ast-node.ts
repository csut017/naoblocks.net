import { RoboLangAstToken } from './ast-token';

export class RoboLangAstNode {
  sourceId: string;
  token: RoboLangAstToken;
  children: RoboLangAstNode[];
  arguments: RoboLangAstNode[];
  type: string;

  statusIcon: string;
}
