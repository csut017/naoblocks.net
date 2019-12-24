export class ExecutionResult<T> {
    successful: boolean;
    validationErrors: string[];
    executionErrors: string[];
    output: T;

    constructor(error?: string) {
        if (error) {
            this.executionErrors = [error];
        }
    }

    allErrors(): string[] {
        return (this.validationErrors ? this.validationErrors : [])
            .concat((this.executionErrors ? this.executionErrors : []));
    }
}
