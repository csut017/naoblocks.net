import { UserStatus } from './user-status';
import { RobotStatus } from './robot-status';

export class SystemStatus {
    usersConnected: UserStatus[] = [];
    robotsConnected: RobotStatus[] = [];
    error: string;

    constructor(error?: string) {
        this.error = error;
    }
}
