import { ClientMessageType } from '../services/connection.service'

export class RobotLogLine {
    description: string;
    sourceMessageType: ClientMessageType;
    whenAdded: Date;
    values: { [id: string]: string };
}
