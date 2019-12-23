export class ResultSet<T> {
    count: number;
    page: number;
    items: T[];
    errorMsg: string;

    constructor(error?: string) {
        this.items = [];
        this.errorMsg = error;
    }
}
