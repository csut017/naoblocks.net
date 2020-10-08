export class SiteAddress {
    url: string;
    isDefault: boolean = false;

    constructor(address?: string) {
        this.url = address;        
    }
}
