import { ControllerAction } from "./controller-action";

export class ControllerEvent {
    action: ControllerAction;
    data?: any;

    constructor(action: ControllerAction) {
        this.action = action;
    }
}
