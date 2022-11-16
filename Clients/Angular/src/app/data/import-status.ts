import { ImportError } from "./import-error";

export class ImportStatus {
    files: any[] = [];
    isUploadCancelling: boolean = false;
    isUploadCompleted: boolean = false;
    isUploading: boolean = false;
    uploadProgress: number = 0;
    uploadState: number = 0;
    uploadStatus: string = '...';
    errors: ImportError[] = [];

    addError(position: number, message: string) {
        this.errors.push(new ImportError(position, message));
    }
}
