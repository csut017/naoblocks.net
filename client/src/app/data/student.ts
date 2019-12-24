export class Student {
    name: string;
    password: string;
    whenAdded: Date;
    isNew: boolean;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
