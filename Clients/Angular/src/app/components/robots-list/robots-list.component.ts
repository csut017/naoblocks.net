import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Robot } from 'src/app/data/robot';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-robots-list',
  templateUrl: './robots-list.component.html',
  styleUrls: ['./robots-list.component.scss']
})
export class RobotsListComponent implements OnInit {

  columns: string[] = ['select', 'machineName', 'friendlyName', 'type', 'whenAdded', 'isInitialised'];
  dataSource: MatTableDataSource<Robot> = new MatTableDataSource();
  isLoading: boolean = true;
  selection = new SelectionModel<Robot>(true, []);

  constructor(private robotService: RobotService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit(): void {
    this.loadList();
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

  checkboxLabel(row?: Robot): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.machineName}`;
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotService.list()
      .subscribe(data => {
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

}
