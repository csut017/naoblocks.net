import { NamedValue } from './named-value';
import { ParseResult } from './parse-result';
import { RobotLog } from './robot-log';
import { RobotType } from './robot-type';

export class Robot {
    id?: string;
    friendlyName?: string;
    machineName?: string;
    type?: string;
    password?: string;
    whenAdded?: Date;
    isNew: boolean;
    isLoading: boolean = false;
    isInitialised: boolean = false;
    logs: RobotLog[] = [];
    filteredLogs: RobotLog[] = [];

    typeDetails?: RobotType;
    parse?: ParseResult;
    values?: NamedValue[];

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
