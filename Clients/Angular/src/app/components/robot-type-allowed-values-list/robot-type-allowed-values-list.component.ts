import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeletionItems } from 'src/app/data/deletion-items';
import { NamedValue } from 'src/app/data/named-value';
import { NamedValueEdit } from 'src/app/data/named-value-edit';
import { RobotType } from 'src/app/data/robot-type';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { NamedValueEditorService } from 'src/app/services/named-value-editor.service';
import { RobotTypeService } from 'src/app/services/robot-type.service';

@Component({
  selector: 'app-robot-type-allowed-values-list',
  templateUrl: './robot-type-allowed-values-list.component.html',
  styleUrls: ['./robot-type-allowed-values-list.component.scss']
})
export class RobotTypeAllowedValuesListComponent implements OnChanges {
  @Input() item?: RobotType;
  @Output() closed = new EventEmitter<boolean>();

  columns: string[] = ['select', 'name', 'value'];
  dataSource: MatTableDataSource<NamedValue> = new MatTableDataSource();
  isLoading: boolean = true;
  selection = new SelectionModel<NamedValue>(true, []);

  constructor(private robotTypeService: RobotTypeService,
    private authenticationService: AuthenticationService,
    private deleteConfirm: DeletionConfirmationService,
    private editorService: NamedValueEditorService)
     {  }

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
        let values = data.output?.customValues || [];
        this.dataSource = new MatTableDataSource(values);
        this.isLoading = false;
      });
  }

  add() {
    let settings = new NamedValueEdit('Add Allowed Value', new NamedValue('', ''));
    this.editorService.show(settings)
      .subscribe(result => {
        // TODO
      });
  }

  checkboxLabel(row?: NamedValue): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  delete(): void {
    this.deleteConfirm.confirm(new DeletionItems('Allowed Values', this.selection.selected.map(item => item.name || '')))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.doDelete();
      });
  }

  doDelete(): void {
    // TODO
  }

  doClose() {
    this.closed.emit(false);
  }


  edit(): void {
    // TODO
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
}
