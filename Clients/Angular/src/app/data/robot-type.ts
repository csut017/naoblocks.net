import { LoggingTemplate } from "./logging-template";
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
    templates?: LoggingTemplate[];
    parse?: ParseResult;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }

    static setNewStatus(type: RobotType, isNew: boolean) {
        type.isNew = isNew;
        if (!type.isNew) type.id = type.name;
      }
  }
