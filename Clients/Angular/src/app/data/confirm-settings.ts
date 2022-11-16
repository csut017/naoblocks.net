export class ConfirmSettings {
    title: string;
    prompt: string;
    action: string;
    showCancel: boolean;

    constructor(prompt: string, title: string = 'Confirm', action: string = 'Ok', showCancel: boolean = true) {
        this.prompt = prompt;
        this.title = title;
        this.action = action;
        this.showCancel = showCancel;
    }
}
