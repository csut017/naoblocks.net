import { BlockDefinition } from "./block-definition";
import { BlockSet } from "./block-set";
import { SessionChecker } from "./session-checker";

export class RobotTypeBlockDefinitions implements SessionChecker {
    errorMsg?: string;
    blocks?: BlockDefinition[];
    blockSets?: BlockSet[];

    hasSessionExpired: boolean = false;

    constructor(error?: string) {
        this.errorMsg = error;
    }
}
