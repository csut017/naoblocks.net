export class Block {
    action: string
    image: string;
    id: string;

    constructor(image: string, action: string) {
        this.action = action;
        this.image = image;
    }

    generate(id: number): Block {
        let block = new Block(this.image, this.action);
        block.id = 'b_' + id;
        return block;
    }
}
