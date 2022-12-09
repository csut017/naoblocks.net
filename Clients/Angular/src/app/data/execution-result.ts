import { SessionChecker } from "./session-checker";

export class ExecutionResult<T> implements SessionChecker {
    message?: string;
    successful: boolean;
    validationErrors: string[] = [];
    executionErrors: string[] = [];
    output?: T;

    hasSessionExpired: boolean = false;

    constructor(data?: T, error?: string, message?: string) {
        this.output = data;
        this.message = message;
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
