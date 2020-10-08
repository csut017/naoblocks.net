import { Component, EventEmitter, OnInit } from '@angular/core';
import { ResultSet } from '../data/result-set';
import { FileDownloaderService } from '../services/file-downloader.service';
import { forkJoin } from 'rxjs';
import { RobotTypeService } from '../services/robot-type.service';
import { RobotType } from '../data/robot-type';

@Component({
  selector: 'app-robot-types-list',
  templateUrl: './robot-types-list.component.html',
  styleUrls: ['./robot-types-list.component.scss']
})
export class RobotTypesListComponent implements OnInit {

  isLoading: boolean = false;
  isInList: boolean = true;
  isInEditor: boolean = false;
  isNew: boolean = false;
  isUploadCancelling: boolean = false;
  isUploadCompleted: boolean = false;
  isUploading: boolean = false;
  selected: RobotType[] = [];
  robotTypes: ResultSet<RobotType> = new ResultSet<RobotType>();
  currentRobotType: RobotType;
  message: string;
  errorMessage: string;

  importToolboxOpened: boolean = false;
  importPackageOpened: boolean = false;
  files: any[] = [];
  uploadProgress: number = 0;
  uploadProgressEnd: number = 0;
  uploadState: number = 0;
  uploadStatus: string = '...';

  constructor(private robotTypeService: RobotTypeService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew(): void {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = true;
    this.currentRobotType = new RobotType(true);
  }

  doDelete(): void {
    forkJoin(this.selected.map(s => this.robotTypeService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        if (successful.length !== 0) {
          this.message = `Deleted ${successful.length} robot types`;
        } else {
          this.message = undefined;
        }
        if (failed.length !== 0) {
          this.errorMessage = `Failed to delete ${failed.length} robot types`;
        } else {
          this.errorMessage = undefined;
        }

        this.robotTypes.items = this.robotTypes
          .items
          .filter(el => !successful.includes(el));
        this.robotTypes.count -= successful.length;
      });
  }

  doEdit(typeToEdit?: RobotType): void {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = false;
    this.currentRobotType = typeToEdit || this.selected[0];
  }

  doExportList(): void {
    // this.downloaderService.download('v1/robots/export/list', 'robots-list.xlsx');
  }

  doExportDetails(): void {

  }

  cancelUpload(): void {
    if (this.isUploading) {
      this.isUploadCancelling = true;
      return;
    }

    this.importPackageOpened = false;
  }

  doExportPackage(): void {
    this.selected.forEach(rt =>
      this.downloaderService.download(
        `v1/robots/types/export/package/${rt.name}`,
        `robotType-${rt.name}-package.zip`)
    );
  }

  doImportPackage(): void {
    this.errorMessage = undefined;
    this.importPackageOpened = true;
    this.isUploadCompleted = false;
    this.isUploadCancelling = false;
    this.files = [];
  }

  doSendPackage(): void {
    this.isUploading = true;
    this.uploadState = 0;
    this.uploadProgress = 0;
    this.uploadProgressEnd = this.files.length + 1;
    let emitter = new EventEmitter<number>();
    emitter.subscribe((pos: number) => {
      this.uploadProgress = pos;
      if (this.isUploadCancelling) {
        this.uploadStatus = `Cancelled package upload`;
        this.uploadState = 2;
        this.isUploading = false;
        this.isUploadCompleted = true;
        this.uploadProgress = this.uploadProgressEnd;
      }

      if (pos < this.files.length) {
        const file = this.files[pos];
        this.uploadStatus = `Uploading ${file.name}...`;

        const reader = new FileReader();
        reader.onload = (e: any) => {
          console.log('[RobotTypesList] Read package file');
          const data = e.target.result;
          forkJoin(this.selected.map(rt => this.robotTypeService.uploadPackageFile(rt, file.name, data)))
            .subscribe(results => {
              this.uploadStatus = `Uploaded ${file.name}`;
              emitter.emit(pos + 1);
            });

        };
        console.log('[RobotTypesList] Reading package file');
        reader.readAsText(file);
      } else {
        this.uploadStatus = `Generating package list...`;
        this.uploadState = 1;
        forkJoin(this.selected.map(rt => this.robotTypeService.generatePackageList(rt)))
          .subscribe(results => {
            this.uploadStatus = `Completed package upload`;
            this.uploadState = 2;
            this.isUploading = false;
            this.isUploadCompleted = true;
            this.uploadProgress = pos + 1;
          });
      }
    });
    emitter.emit(0);
  }

  doImportToolbox(): void {
    this.errorMessage = undefined;
    this.importToolboxOpened = true;
    this.files = [];
  }

  doSendToolbox(): void {
    // TODO
    // this.importToolboxOpened = false;
    // this.selected.forEach(rt => {
    //   this.robotTypeService.storeToolbox(rt, this.toolboxToUpload)
    //     .subscribe(result => {
    //       alert(result.successful ? 'Successful' : 'Failed');
    //     });
    // });
  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    this.isInList = true;
    if (saved) {
      if (this.isNew) {
        this.robotTypes.items.push(this.currentRobotType);
        this.message = `Added robot type '${this.currentRobotType.name}'`;
        this.currentRobotType.id = this.currentRobotType.name;
      } else {
        this.message = `Updated robot type '${this.currentRobotType.name}'`;
      }
    }
  }

  doRefresh(): void {
    this.loadList();
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotTypeService.list()
      .subscribe(data => {
        this.robotTypes = data;
        this.isLoading = false;
      });
  }

  fileBrowseHandler($event): void {
    for (const file of $event) {
      this.files.push(file);
    }
  }

  deleteFile(index: number) {
    this.files.splice(index, 1);
  }

  formatBytes(bytes: number, decimals: number = 2): string {
    if (bytes === 0) {
      return "0 Bytes";
    }
    const k = 1024;
    const dm = decimals <= 0 ? 0 : decimals;
    const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
  }
}
