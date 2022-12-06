import { NamedValue } from "./named-value";
import { Toolbox } from "./toolbox";

export class RobotType {
    id?: string;
    name?: string;
    isDefault: boolean = false;
    allowDirectLogging: boolean = false;
    hasToolbox: boolean = false;
    isNew: boolean;
    
    customValues?: NamedValue[];
    toolboxes?: Toolbox[];
    message?: string;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
