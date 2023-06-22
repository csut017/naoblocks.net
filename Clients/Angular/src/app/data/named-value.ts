export class NamedValue {
    name: string;
    value: string;
    default?: string;

    constructor(name: string, value: string) {
        this.name = name;
        this.value = value;
    }
}
