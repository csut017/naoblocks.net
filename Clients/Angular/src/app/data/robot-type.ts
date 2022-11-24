import { Toolbox } from "./toolbox";

export class RobotType {
    id?: string;
    name?: string;
    isDefault: boolean = false;
    allowDirectLogging: boolean = false;
    hasToolbox: boolean = false;
    isNew: boolean;
    toolboxes?: Toolbox[];

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
