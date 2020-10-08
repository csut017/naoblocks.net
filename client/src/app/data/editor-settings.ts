import { UserSettings } from './user-settings';

export class EditorSettings {
    user: UserSettings = new UserSettings();
    toolbox: string;
    isSystemInitialised: boolean;
    canConfigure: boolean;

    isLoaded: boolean = false;
}
