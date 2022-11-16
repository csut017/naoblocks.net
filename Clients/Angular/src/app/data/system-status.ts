import { UserStatus } from './user-status';
import { RobotStatus } from './robot-status';
import { SessionChecker } from './session-checker';

export class SystemStatus implements SessionChecker {
    usersConnected: UserStatus[] = [];
    robotsConnected: RobotStatus[] = [];
    error?: string;

    hasSessionExpired: boolean = false;

    constructor(error?: string) {
        this.error = error;
    }
}
