import { RobotLog } from './robot-log';

export class Robot {
    id?: string;
    friendlyName?: string;
    machineName?: string;
    type?: string;
    password?: string;
    message?: string;
    whenAdded?: Date;
    isNew: boolean;
    isLoading: boolean = false;
    isInitialised: boolean = false;
    logs: RobotLog[] = [];
    filteredLogs: RobotLog[] = [];

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
