export class ImportStatus {
    files: any[] = [];
    isUploadCancelling: boolean = false;
    isUploadCompleted: boolean = false;
    isUploading: boolean = false;
    uploadProgress: number = 0;
    uploadState: number = 0;
    uploadStatus: string = '...';
}
