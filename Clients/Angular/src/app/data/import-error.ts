export class ImportError {
    position: number;
    message: string;

    constructor(position: number, error: string) {
        this.position = position;
        this.message = error;
    }
}
