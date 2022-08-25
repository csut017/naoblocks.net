import { DomSanitizer, SafeResourceUrl } from "@angular/platform-browser";
import { TangibleDefinition } from "./tangible-definition";

declare var Tangibles: any;

export class Block {
    id: string = '';
    highlight: boolean = false;

    constructor(public image: string, 
        public text: string, 
        public name: string,
        private sanitizer: DomSanitizer) {
    }

    static initialise(id: number, definition: TangibleDefinition, sanitizer: DomSanitizer): Block {
        let block = new Block(definition.image, definition.text, definition.type, sanitizer);
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

    imageData(): SafeResourceUrl {
        return this.sanitizer.bypassSecurityTrustResourceUrl(this.image);
    }
}
