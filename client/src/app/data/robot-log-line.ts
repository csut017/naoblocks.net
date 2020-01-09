export class RobotLogLine {
    description: string;
    sourceMessageType: string;
    whenAdded: Date;
    icon: string;
    values: { [id: string]: string };
    skip: boolean;
}
