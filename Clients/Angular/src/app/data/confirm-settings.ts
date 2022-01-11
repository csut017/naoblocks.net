export class ConfirmSettings {
    title: string;
    prompt: string;
    action: string;

    constructor(prompt: string, title: string = 'Confirm', action: string = 'Ok') {
        this.prompt = prompt;
        this.title = title;
        this.action = action;
    }
}
