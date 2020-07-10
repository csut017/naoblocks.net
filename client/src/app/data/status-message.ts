export class StatusMessage {
    type: string;
    message: string;

    displayMessage(): string {
        return this.message;
    }
}
