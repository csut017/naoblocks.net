export class ResultSet<T> {
    count: number = 0;
    page: number = 0;
    items: T[];
    errorMsg?: string;

    constructor(error?: string) {
        this.items = [];
        this.errorMsg = error;
    }
}
