import { ImportStatus } from "./import-status";

export class ImportSettings<Type> {
    title: string = 'Import';
    prompt: string = '';
    action: string = 'Import';
    showCancel: boolean = true;
    allowMultiple: boolean = false;
    uploadAction: (status: ImportStatus, settings: ImportSettings<Type>) => void;
    items: Type[];
    owner: any;

    constructor(items: Type[], action: (status: ImportStatus, settings: ImportSettings<Type>) => void, owner: any) {
        this.uploadAction = action;
        this.items = items;
        this.owner = owner;
    }
}
