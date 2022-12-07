import { NamedValue } from "./named-value";
import { ParseResult } from "./parse-result";
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
    parse?: ParseResult;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
