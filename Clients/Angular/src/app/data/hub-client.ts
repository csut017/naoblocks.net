import { NotificationAlert } from './notification-alert';
import { StatusMessage } from './status-message';

export class HubClient {
    id?: number;
    name?: string;
    type?: string;
    subType?: string;

    // User data
    student: boolean = false;
    robot?: HubClient;
    
    // Robot data
    messages: StatusMessage[] = [];
    programId?: number;
    status?: string;
    user?: HubClient;

    notifications: NotificationAlert[] = [];
    hasNotifications: boolean = false;
}
