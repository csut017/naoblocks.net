export class Robot {
    id: string;
    friendlyName: string;
    machineName: string;
    whenAdded: Date;
    isNew: boolean;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
