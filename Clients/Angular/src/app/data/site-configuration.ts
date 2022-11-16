import { SessionChecker } from "./session-checker";

export class SiteConfiguration implements SessionChecker {
    defaultAddress?: string;

    hasSessionExpired: boolean = false;
}
