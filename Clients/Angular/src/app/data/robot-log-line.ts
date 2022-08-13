export class RobotLogLine {
    description?: string;
    sourceMessageType?: Number;
    whenAdded?: Date;
    icon?: string;
    values: { [id: string]: string } = {};
    isDebug: boolean = false;
}
