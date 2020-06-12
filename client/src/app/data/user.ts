import { UserSettings } from './user-settings';

export class User {
    id: string;
    name: string;
    password: string;
    whenAdded: Date;
    isNew: boolean;
    settings: UserSettings;
    isFullyLoaded: boolean = false;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
