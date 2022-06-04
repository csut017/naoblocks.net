import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { ExecutionResult } from 'src/app/data/execution-result';
import { ImportSettings } from 'src/app/data/import-settings';
import { ImportStatus } from 'src/app/data/import-status';
import { UIDefinition } from 'src/app/data/ui-definition';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { ImportService } from 'src/app/services/import.service';
import { UiService } from 'src/app/services/ui.service';

@Component({
  selector: 'app-user-interface-configuration',
  templateUrl: './user-interface-configuration.component.html',
  styleUrls: ['./user-interface-configuration.component.scss']
})
export class UserInterfaceConfigurationComponent implements OnInit {

  columns: string[] = ['select', 'key', 'name', 'description', 'isInitialised'];
  dataSource: MatTableDataSource<UIDefinition> = new MatTableDataSource();
  isLoading: boolean = true;
  selection = new SelectionModel<UIDefinition>(true, []);
  view: string = 'list';

  constructor(private uiService: UiService,
    private importService: ImportService,
    private authenticationService: AuthenticationService
  ) { }

  ngOnInit(): void {
    this.loadList();
  }

  import(): void {
    const definition = this.selection.selected[0];
    let settings = new ImportSettings(this.selection.selected, this.doImportDefinition, this);
    settings.prompt = `Select the definition to import for ${definition.name}:`;
    settings.title = 'Import UI Definition';
    this.importService.start(settings);
  }

  doImportDefinition(status: ImportStatus, settings: ImportSettings<UIDefinition>): void {
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
        reader.onload = (e: any) => {
          console.log('[UserInterfaceConfigurationComponent] Read definition file');
          const data = e.target.result;
          const definition = settings.items[0]; // There should be only one definition
          const service: UiService = settings.owner.uiService;
          service.import(definition, data, true)
            .subscribe((results: ExecutionResult<UIDefinition>) => {
              if (!results.successful) {
                results.allErrors().forEach((err: string) => status.addError(0, err));
              }

              emitter.emit(pos + 1);
            });
        };
        console.log('[UserInterfaceConfigurationComponent] Reading definition file');
        reader.readAsText(file);
      } else {
        if (status.errors.length == 0) {
          status.uploadStatus = `Imported UI definition`;
          status.uploadState = 2;
        } else {
          status.uploadStatus = `UI definition import failed: ` + status.errors.map(err => err.message).join(',');
          status.uploadState = 3;
          settings.allowReImport = true;
        }
        status.isUploading = false;
        status.isUploadCompleted = true;
        status.uploadProgress = 100;
      }
    });
    emitter.emit(0);
  }

  checkboxLabel(row?: UIDefinition): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.key}`;
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

  show(): void {
    this.view = 'item';
  }

  private loadList(): void {
    this.isLoading = true;
    this.uiService.list()
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data)) return;
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

}
