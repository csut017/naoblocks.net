import { CompilationError } from './compilation-error';
import { RoboLangAstNode } from './ast-node';

export class Compilation {
    programId: string
    errors: CompilationError[];
    nodes: RoboLangAstNode[];
}
