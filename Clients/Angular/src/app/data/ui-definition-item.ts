export class UIDefinitionItem {
    name?: string;
    description?: string;
    image?: string;
    children: UIDefinitionItem[] = [];
    isOpen: boolean = false;
}
