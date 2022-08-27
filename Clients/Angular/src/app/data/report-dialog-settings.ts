import { NamedValue } from "./named-value";

export class ReportDialogSettings {
    allowedFormats: NamedValue[] = [
        new NamedValue('Text', 'txt')
    ];
    showDateRange: boolean;
    title: string;

    constructor(title: string, showDateRange: boolean = false) {
        this.showDateRange = showDateRange;
        this.title = title;
    }

    addExcelFormat(): void {
        
    }
}
