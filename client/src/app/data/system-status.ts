import { UserStatus } from './user-status';
import { RobotStatus } from './robot-status';

export class SystemStatus {
    usersConnected: UserStatus[] = [];
    robotsConnected: RobotStatus[] = [];
    status: string;

    constructor(status?: string) {
        this.status = status;
    }
}
