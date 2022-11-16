import { StatusMessage } from './status-message';

export class DebugMessage extends StatusMessage {
    function?: string;
    sourceID?: string;
    status?: string;
    programId?: number;

    constructor() {
        super();
        this.type = 'details';
    }

    override displayMessage(): string {
        return `Debug: ${this.function} [${this.status}]`;
    }
}
