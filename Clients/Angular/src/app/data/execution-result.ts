export class ExecutionResult<T> {
    successful: boolean;
    validationErrors: string[] = [];
    executionErrors: string[] = [];
    output?: T;

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
