export class UserSettings {
    simple: boolean = false;
    events: boolean = false;

    conditionals: boolean = false;
    dances: boolean = false;
    loops: boolean = false;
    sensors: boolean = false;
    tutorials: boolean = false;
    variables: boolean = false;

    robotType?: string;
    robotId?: string;
    allocationMode?: number;

    customBlockSet?: string;
}
