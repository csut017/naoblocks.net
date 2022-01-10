import { SessionChecker } from "./session-checker";

export class ResultSet<T> implements SessionChecker {
    count: number = 0;
    page: number = 0;
    items: T[];
    errorMsg?: string;

    hasSessionExpired: boolean = false;

    constructor(error?: string) {
        this.items = [];
        this.errorMsg = error;
    }
}
