import { NamedValue } from "./named-value";
import { ReportFlag } from "./report-flag";

export class ReportDialogSettings {
    static readonly Csv: NamedValue = new NamedValue('CSV', 'csv');
    static readonly Excel: NamedValue = new NamedValue('Excel', 'xlsx');
    static readonly Json: NamedValue = new NamedValue('JSON', 'json');
    static readonly Png: NamedValue = new NamedValue('PNG Image', 'png');
    static readonly Pdf: NamedValue = new NamedValue('PDF', 'pdf');
    static readonly Text: NamedValue = new NamedValue('Text', 'txt');
    static readonly Xml: NamedValue = new NamedValue('XML', 'xml');

    allowedFormats: NamedValue[] = [
        ReportDialogSettings.Text
    ];
    flags: ReportFlag[] = [];
    showDateRange: boolean;
    title: string;

    constructor(title: string, showDateRange: boolean = false) {
        this.showDateRange = showDateRange;
        this.title = title;
    }
}
