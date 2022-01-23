export class Block {
    id: string = '';
    highlight: boolean = false;

    constructor(public image: string, 
        public text: string, 
        public action: string) {
    }

    initialise(id: number): Block {
        let block = new Block(this.image, this.text, this.action);
        block.id = 'b_' + id;
        return block;
    }

    generateCode(forRobot: boolean): string {
        if (!forRobot || !this.id) return this.action;
        return `[${this.id}]${this.action}`;
    }
}
