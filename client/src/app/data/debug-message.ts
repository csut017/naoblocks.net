import { StatusMessage } from './status-message';

export class DebugMessage extends StatusMessage {
    function: string;
    sourceID: string;
    status: string;

    constructor() {
        super();
        this.type = 'details';
    }

    displayMessage(): string {
        return `Debug: ${this.function} [${this.status}]`;
    }
}
