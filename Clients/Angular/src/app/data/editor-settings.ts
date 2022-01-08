import { UserSettings } from './user-settings';

export class EditorSettings {
    user: UserSettings = new UserSettings();
    toolbox?: string;
    isSystemInitialised: boolean = false;
    canConfigure: boolean = false;

    isLoaded: boolean = false;
}
