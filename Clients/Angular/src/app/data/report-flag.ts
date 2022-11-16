export class ReportFlag {
    name: string;
    text: string;
    value: boolean;

    constructor(name: string, text?: string, value?: boolean) {
        this.name = name;
        this.text = text || name;
        this.value = value || false;
    }
}