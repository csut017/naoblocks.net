import { DateTime } from "luxon";
import { NamedValue } from "./named-value";

export class ReportSettings {
    dateFrom?: DateTime;
    dateTo?: DateTime;
    selectedFormat?: NamedValue;
    flags: string[] = [];

    encodeFlags(): string {
        let allFlags = this.flags.join(',');
        return encodeURIComponent(allFlags);
    }
}
