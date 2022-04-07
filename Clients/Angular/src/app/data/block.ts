import { TangibleDefinition } from "./tangible-definition";

declare var Tangibles: any;

export class Block {
    id: string = '';
    highlight: boolean = false;

    constructor(public image: string, 
        public text: string, 
        public name: string) {
    }

    static initialise(id: number, definition: TangibleDefinition): Block {
        let block = new Block(definition.image, definition.text, definition.type);
        block.id = 'b_' + id;
        return block;
    }

    generateCode(forRobot: boolean): string {
        let name = this.name,
            generator = Tangibles.NaoLang[name];
        if (!generator) {
            throw 'Unknown block: ' + name;
        }
        let code = generator(this);
        
        if (!forRobot || !this.id) return code;
        return `[${this.id}]${code}`;
    }

    imageData(): string {
        return 'data:image/png;base64,' + this.image;
    }
}
