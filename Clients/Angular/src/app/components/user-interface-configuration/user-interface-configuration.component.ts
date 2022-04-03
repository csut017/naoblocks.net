import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { UIDefinition } from 'src/app/data/ui-definition';
import { AuthenticationService } from 'src/app/services/authentication.service';
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
    private authenticationService: AuthenticationService
    ) { }

  ngOnInit(): void {
    this.loadList();
  }

  import(): void {

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
