import { UserSettings } from './user-settings';

export class Student {
    id: string;
    name: string;
    password: string;
    whenAdded: Date;
    isNew: boolean;
    settings: UserSettings;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
