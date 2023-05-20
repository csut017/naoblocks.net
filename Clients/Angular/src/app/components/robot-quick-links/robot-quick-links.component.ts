import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Robot } from 'src/app/data/robot';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { RobotService } from 'src/app/services/robot.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-robot-quick-links',
  templateUrl: './robot-quick-links.component.html',
  styleUrls: ['./robot-quick-links.component.scss']
})
export class RobotQuickLinksComponent implements OnChanges {
  @Input() item?: Robot;
  @Output() closed = new EventEmitter<boolean>();

  form: UntypedFormGroup;
  imageUrl: string = '';
  quickLinkUrl?: string;

  formats: string[] = ['QR Code'];
  types: string[] = ['Mobile View', 'Editor'];

  constructor(private robotService: RobotService,
    private downloaderService: FileDownloaderService) {
    this.form = new UntypedFormGroup({
      format: new UntypedFormControl(this.formats[0], [Validators.required]),
      type: new UntypedFormControl(this.types[0], [Validators.required]),
    });

    this.form.valueChanges.subscribe(_ => this.updateUrl());
  }

  ngOnChanges(_: SimpleChanges): void {
    this.updateUrl();
  }
  
  doClose() {
    this.closed.emit(false);
  }

  doDownload(): void {
    let format = this.form.get('format')?.getRawValue();
    let type = this.form.get('type')?.getRawValue();
    this.downloaderService.download(
      this.generateUrl(),
      `${format}-${this.item?.machineName}-${type}.png`);
  }

  updateUrl(): void {
    let url = this.generateUrl();
    this.imageUrl = environment.apiURL + url;
    if (!this.item?.machineName) return;
    let type = this.form.get('type')?.getRawValue()?.replace(/\s/g, "")?.toLowerCase();
    this.robotService.getQuickLink(this.item.machineName, type)
      .subscribe(res => this.quickLinkUrl = res.output?.link);
  }


  private generateUrl() {
    let format = this.form.get('format')?.getRawValue()?.replace(/\s/g, "")?.toLowerCase();
    let type = this.form.get('type')?.getRawValue()?.replace(/\s/g, "")?.toLowerCase();
    return `v1/robots/${this.item?.machineName}/${format}/${type}`;
  }
}
