import { NamedValue } from "./named-value";

export class NamedValueEdit {
    title: string;
    action: string = 'Save';
    value: NamedValue;

    constructor(title: string, value: NamedValue) {
        this.title = title;
        this.value = value;
    }
}
