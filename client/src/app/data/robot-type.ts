export class RobotType {
    id: string;
    name: string;
    isDefault: boolean;
    isNew: boolean;

    constructor(isNew: boolean = false) {
        this.isNew = isNew;
    }
}
