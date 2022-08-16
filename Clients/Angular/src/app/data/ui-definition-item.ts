export class UIDefinitionItem {
    name?: string;
    description?: string;
    children: UIDefinitionItem[] = [];
    isOpen: boolean = false;
}
