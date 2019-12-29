import { ProgramFile } from './program-file';

export class ProgramDirectory {
    name: string;
    expanded: boolean = false;
    files: ProgramFile[]
}
