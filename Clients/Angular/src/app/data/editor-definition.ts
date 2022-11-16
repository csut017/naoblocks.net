export class EditorDefinition {
    name: string;
    title: string;
    fileCommands: boolean;

    constructor(name: string, title: string, fileCommands: boolean) {
        this.name = name;
        this.title = title;
        this.fileCommands = fileCommands;
    }
}
