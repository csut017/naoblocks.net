export class SystemVersion {
    version: string;
    error: string;

    constructor(error?: string) {
        this.error = error;
    }
}
