import { UserSettings } from './user-settings';

export class EditorSettings {
    user: UserSettings = new UserSettings();
    toolbox: string;

    isLoaded: boolean = false;
}
