import { UserSettings } from './user-settings';

export class Student {
    id: string;
    name: string;
    password: string;
    whenAdded: Date;
    isNew: boolean;
    settings: UserSettings;
    age: number;
    gender: string;
    isFullyLoaded: boolean = false;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
