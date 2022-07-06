import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { DeletionItems } from 'src/app/data/deletion-items';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
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

  columns: string[] = ['select', 'name', 'isDefault' ];
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
    private snackBar: MatSnackBar,
    private multilineMessage: MultilineMessageService) {}

  ngOnInit(): void {
  }

  ngOnChanges(_: SimpleChanges): void {
    if (!this.item) return;

    this.isLoading = true;
    this.robotTypeService.get(this.item.id!)
    .subscribe(resp => {

    });

    this.robotTypeService.get(this.item.id!)
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data)) return;
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

  checkboxLabel(row?: RobotType): string {
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
    forkJoin(this.selection.selected.map(s => this.robotTypeService.deleteToolbox(this.item!, s.id!)))
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

  private generateCountText(count: number): string {
    const text = count == 1
      ? '1 toolbox'
      : `${count} toolboxes`
    return text;
  }
}
