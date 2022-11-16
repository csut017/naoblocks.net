import { DateTime } from "luxon";
import { NamedValue } from "./named-value";

export class ReportSettings {
    dateFrom?: DateTime;
    dateTo?: DateTime;
    selectedFormat?: NamedValue;
}
