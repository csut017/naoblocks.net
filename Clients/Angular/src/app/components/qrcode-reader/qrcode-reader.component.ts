import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-qrcode-reader',
  templateUrl: './qrcode-reader.component.html',
  styleUrls: ['./qrcode-reader.component.scss']
})
export class QRCodeReaderComponent {
  availableDevices: MediaDeviceInfo[] = [];
  deviceCurrent?: MediaDeviceInfo = undefined;
  deviceSelected?: string = undefined;
  qrCodesEnabled: boolean = false;

  @Output() onKeyFound = new EventEmitter<string>();

  onCamerasFound(devices: MediaDeviceInfo[]): void {
    this.availableDevices = devices;
  }

  onCodeResult(resultString: string) {
    let url = new URL(resultString);
    console.log({
      raw: resultString,
      url: url
    });
    if ((url.pathname == '/login') && url.search.startsWith('?key=')) {
      this.onKeyFound.emit(url.search.substring(5));
    }
  }

  onDeviceChange(device: MediaDeviceInfo) {
    const selectedStr = device?.deviceId || '';
    if (this.deviceSelected === selectedStr) { return; }
    this.deviceSelected = selectedStr;
    this.deviceCurrent = device || undefined;
  }
  
  onDeviceSelectChange(selected: string) {
    const selectedStr = selected || '';
    if (this.deviceSelected === selectedStr) { return; }
    this.deviceSelected = selectedStr;
    const device = this.availableDevices.find(x => x.deviceId === selected);
    this.deviceCurrent = device || undefined;
  }  
}
