import { DebugMessage } from './debug-message';
import { NumberValueAccessor } from '@angular/forms';

export class HubClient {
    id: number;
    name: string;
    type: string;
    subType: string;

    // User data
    student: boolean;
    robot: HubClient;
    
    // Robot data
    status: string;
    messages: DebugMessage[] = [];
}
