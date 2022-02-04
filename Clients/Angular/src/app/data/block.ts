import { TangibleDefinition } from "./TangibleDefinition";

export class Block {
    id: string = '';
    highlight: boolean = false;

    constructor(public image: string, 
        public text: string, 
        public action: string) {
    }

    static initialise(id: number, definition: TangibleDefinition): Block {
        let block = new Block(definition.image, definition.text, definition.action);
        block.id = 'b_' + id;
        return block;
    }

    generateCode(forRobot: boolean): string {
        if (!forRobot || !this.id) return this.action;
        return `[${this.id}]${this.action}`;
    }

    imageData(): string {
        return 'data:image/png;base64,' + this.image;
    }
}
