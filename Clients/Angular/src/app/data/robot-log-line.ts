export class RobotLogLine {
    description?: string;
    sourceMessageType?: Number;
    whenAdded?: Date;
    icon?: string;
    iconClass?: string;
    hasValues: boolean = false;
    values: { [id: string]: string } = {};
    isOpen: boolean = false;
}
