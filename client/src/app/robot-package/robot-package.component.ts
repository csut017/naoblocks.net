import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { RobotType } from '../data/robot-type';
import { RobotTypeService } from '../services/robot-type.service';
import { PackageFile } from '../data/package-file';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-robot-package',
  templateUrl: './robot-package.component.html',
  styleUrls: ['./robot-package.component.scss']
})
export class RobotPackageComponent implements OnInit {

  @Input() robotType: RobotType;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];
  downloadingFileList: boolean = true;
  files: PackageFile[] = [];
  downloadPackageUrl: string = '';

  constructor(private robotTypeService: RobotTypeService) { }

  ngOnInit() {
    this.downloadPackageUrl = `${environment.apiURL}v1/robots/types/${this.robotType.id}/files.txt`;
    this.robotTypeService.listPackageFiles(this.robotType)
      .subscribe(files => {
        this.files = files.items;
        this.downloadingFileList = false;
      });
  }

  doCancel() {
    this.closed.emit(false);
  }

  generateDownloadLink(file: PackageFile): string {
    const fileName = encodeURIComponent(file.name);
    return `${environment.apiURL}v1/robots/types/${this.robotType.id}/files/${fileName}`;
  }

}
