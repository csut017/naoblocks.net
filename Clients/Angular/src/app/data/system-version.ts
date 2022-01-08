export class SystemVersion {
    version?: string;
    status?: string;
    error?: string;

    constructor(error?: string) {
        this.error = error;
    }
}
