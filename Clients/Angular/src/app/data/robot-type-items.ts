export class RobotTypeItems {
    templates: boolean;
    toolboxes: boolean;
    values: boolean;

    constructor(
        templates: boolean = false,
        toolboxes: boolean = false,
        values: boolean = false) {
        this.templates = templates;
        this.toolboxes = toolboxes;
        this.values = values;
    }
}
