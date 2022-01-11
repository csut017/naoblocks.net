export class DeletionItems {
    type: string;
    pluralType?: string;
    items: string[];

    constructor(type: string, items: string[]) {
        this.type = type;
        this.items = items;
    }
}
