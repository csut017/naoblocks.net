import { StatusMessage } from './status-message';

export class HubClient {
    id: number;
    name: string;
    type: string;
    subType: string;

    // User data
    student: boolean;
    robot: HubClient;
    
    // Robot data
    messages: StatusMessage[] = [];
    programId: number;
    status: string;
    user: HubClient;
}
