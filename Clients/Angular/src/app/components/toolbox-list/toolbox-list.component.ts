import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { DeletionItems } from 'src/app/data/deletion-items';
import { ExecutionResult } from 'src/app/data/execution-result';
import { ImportSettings } from 'src/app/data/import-settings';
import { ImportStatus } from 'src/app/data/import-status';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { ImportService } from 'src/app/services/import.service';
import { MultilineMessageService } from 'src/app/services/multiline-message.service';
import { RobotTypeService } from 'src/app/services/robot-type.service';

@Component({
  selector: 'app-toolbox-list',
  templateUrl: './toolbox-list.component.html',
  styleUrls: ['./toolbox-list.component.scss']
})
export class ToolboxListComponent implements OnInit, OnChanges {

  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  columns: string[] = ['select', 'name', 'isDefault'];
  currentItem?: Toolbox;
  dataSource: MatTableDataSource<Toolbox> = new MatTableDataSource();
  hasDefault: boolean = false;
  isLoading: boolean = true;
  isNew: boolean = false;
  selection = new SelectionModel<Toolbox>(true, []);
  view: string = 'list';

  constructor(private robotTypeService: RobotTypeService,
    private authenticationService: AuthenticationService,
    private deleteConfirm: DeletionConfirmationService,
    private importService: ImportService,
    private snackBar: MatSnackBar,
    private downloaderService: FileDownloaderService,
    private multilineMessage: MultilineMessageService) { }

  ngOnInit(): void {
  }

  ngOnChanges(_: SimpleChanges): void {
    this.loadTemplateList();
  }

  private loadTemplateList(): void {
    if (!this.item) return;
    this.isLoading = true;
    this.robotTypeService.get(this.item.id!)
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data))
          return;
        let toolboxes = data.output?.toolboxes || [];
        this.dataSource = new MatTableDataSource(toolboxes);
        this.isLoading = false;
        this.hasDefault = !!toolboxes.find(rt => rt.isDefault);
      });
  }

  add() {
    this.view = 'editor';
    this.isNew = true;
    this.currentItem = new Toolbox();
  }

  checkboxLabel(row?: Toolbox): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  delete(): void {
    this.deleteConfirm.confirm(new DeletionItems('toolboxes', this.selection.selected.map(item => item.name || '')))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.doDelete();
      });
  }

  doDelete(): void {
    forkJoin(this.selection.selected.map(s => this.robotTypeService.deleteToolbox(this.item!, s.name!)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        let messages: string[] = [];
        if (successful.length !== 0) {
          messages.push(`Deleted ${this.generateCountText(successful.length)}`);
        }

        if (failed.length !== 0) {
          messages.push(`Failed to delete ${this.generateCountText(failed.length)}`);
        }

        this.multilineMessage.show(messages);
        this.selection.clear();
        this.dataSource.data = this.dataSource
          .data
          .filter(el => !successful.includes(el));
        this.updateDefaultRobotMessage();
      });
  }

  doClose() {
    this.closed.emit(false);
  }

  edit(): void {
    this.view = 'editor';
    this.isNew = false;
    this.currentItem = this.selection.selected[0];
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.selection.select(...this.dataSource.data);
  }

  onClosed(saved: boolean) {
    this.view = 'list';
    if (saved) {
      if (this.isNew) {
        this.dataSource.data = [...this.dataSource.data, this.currentItem!];
        this.snackBar.open(`Added toolbox '${this.currentItem!.name}'`);
      } else {
        this.snackBar.open(`Updated toolbox '${this.currentItem!.name}'`);
      }
      this.currentItem!.id = this.currentItem!.name;
      this.updateDefaultRobotMessage();
    }
  }

  updateDefaultRobotMessage() {
    this.hasDefault = !!this.dataSource.data.find(rt => rt.isDefault);
  }

  importToolbox() {
    let settings = new ImportSettings<RobotType>([this.item!], this.doImportToolbox, this);
    settings.prompt = `Select the toolboxes to import for ${this.item?.name}:`;
    settings.allowMultiple = true;
    settings.title = 'Import Robot Type Toolbox';
    this.importService.start(settings);
  }

  doImportToolbox(status: ImportStatus, settings: ImportSettings<RobotType>): void {
    const multipler = 100 / status.files.length;
    let emitter = new EventEmitter<number>();
    emitter.subscribe((pos: number) => {
      status.uploadProgress = pos * multipler;
      if (status.isUploadCancelling) {
        status.uploadStatus = `Cancelled upload`;
        status.uploadState = 2;
        status.isUploading = false;
        status.isUploadCompleted = true;
        status.uploadProgress = 100;
      }

      if (pos < status.files.length) {
        const file = status.files[pos];
        status.uploadStatus = `Uploading ${file.name}...`;

        const reader = new FileReader();
        let toolboxName = file.name;
        if (toolboxName.substring(toolboxName.length - 4).toLowerCase() == '.xml') toolboxName = toolboxName.substring(0, toolboxName.length - 4);
        if (toolboxName.substring(toolboxName.length - 8).toLowerCase() == '-toolbox') toolboxName = toolboxName.substring(0, toolboxName.length - 8);
        reader.onload = (e: any) => {
          console.log('[ToolboxListComponent] Read definition file');
          const data = e.target.result;
          const definition = settings.items[0]; // There should be only one definition
          const service: RobotTypeService = settings.owner.robotTypeService;
          service.importToolbox(
            settings.owner.item,
            toolboxName,
            data,
            false,
            false)
            .subscribe((results: ExecutionResult<Toolbox>) => {
              if (!results.successful) {
                results.allErrors().forEach((err: string) => status.addError(0, err));
              }

              emitter.emit(pos + 1);
            });
        };
        console.log(`[ToolboxListComponent] Reading toolbox file ${file.name}`);
        reader.readAsText(file);
      } else {
        if (status.errors.length == 0) {
          status.uploadStatus = `Imported toolboxes`;
          status.uploadState = 2;
        } else {
          status.uploadStatus = `Toolbox import failed: ` + status.errors.map(err => err.message).join(',');
          status.uploadState = 3;
          settings.allowReImport = true;
        }
        status.isUploading = false;
        status.isUploadCompleted = true;
        status.uploadProgress = 100;
        this.loadTemplateList();
      }
    });
    emitter.emit(0);
  }

  exportToolbox() {
    this.selection.selected.forEach(t => {
      const url = `v1/robots/types/${this.item?.id}/toolbox/${t.name}/export`
      this.downloaderService.download(url, `${t.name}-toolbox.xml`);
    });
  }

  private generateCountText(count: number): string {
    const text = count == 1
      ? '1 toolbox'
      : `${count} toolboxes`
    return text;
  }
}
