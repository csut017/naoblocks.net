import { UserSettings } from './user-settings';

export class EditorSettings {
    user: UserSettings = new UserSettings();
    toolbox?: string;
    useEvents: boolean = false;
    isSystemInitialised: boolean = false;
    canConfigure: boolean = false;

    isLoaded: boolean = false;
}
