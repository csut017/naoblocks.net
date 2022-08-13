import { RobotLogLine } from './robot-log-line';

export class RobotLog {
    conversationId?: number;
    lines: RobotLogLine[] = [];
    whenAdded?: Date;
    whenLastUpdated?: Date;
    selected: boolean = false;
    type: number = 0;
    typeIcon: string = 'device_unknown';
}
