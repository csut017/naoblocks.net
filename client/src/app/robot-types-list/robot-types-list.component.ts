import { Component, OnInit } from '@angular/core';
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
  selected: RobotType[] = [];
  robotTypes: ResultSet<RobotType> = new ResultSet<RobotType>();
  currentRobotType: RobotType;
  message: string;
  errorMessage: string;

  importToolboxOpened: boolean = false;
  importPackageOpened: boolean = false;

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
  }

  doSendPackage(): void {

  }

  doImportToolbox(): void {
    this.errorMessage = undefined;
    this.importToolboxOpened = true;
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

  fileBrowseHandler(): void {
    
  }

}
