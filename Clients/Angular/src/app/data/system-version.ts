import { SessionChecker } from "./session-checker";

export class SystemVersion implements SessionChecker {
    version?: string;
    status?: string;
    error?: string;

    hasSessionExpired: boolean = false;

    constructor(error?: string) {
        this.error = error;
    }
}
