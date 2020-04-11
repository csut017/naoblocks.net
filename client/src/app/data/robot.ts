import { RobotLog } from './robot-log';

export class Robot {
    id: string;
    friendlyName: string;
    machineName: string;
    type: string;
    password: string;
    whenAdded: Date;
    isNew: boolean;
    isLoading: boolean;
    isInitialised: boolean;
    logs: RobotLog[];

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
