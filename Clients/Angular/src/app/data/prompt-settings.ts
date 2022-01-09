import { UserInputMode } from "./user-input-mode";

export class PromptSettings {
    open: boolean = false;
    title: string = '';
    value: string = '';
    action: any;
    showOk: boolean = false;
    showValue: boolean = false;
    mode: UserInputMode = UserInputMode.prompt;
  }
