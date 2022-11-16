import { SessionChecker } from "./session-checker";

export class ExecutionResult<T> implements SessionChecker {
    successful: boolean;
    validationErrors: string[] = [];
    executionErrors: string[] = [];
    output?: T;

    hasSessionExpired: boolean = false;

    constructor(data?: T, error?: string) {
        this.output = data;
        if (error) {
            this.executionErrors = [error];
            this.successful = false;
        } else {
            this.successful = true;
        }
    }

    allErrors(): string[] {
        return (this.validationErrors ? this.validationErrors : [])
            .concat((this.executionErrors ? this.executionErrors : []));
    }
}
