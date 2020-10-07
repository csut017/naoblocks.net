import { UserSettings } from './user-settings';

export class User {
    id: string;
    name: string;
    password: string;
    role: string;
    whenAdded: Date;
    isNew: boolean;
    settings: UserSettings;
    isFullyLoaded: boolean = false;

    hasError: boolean;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
