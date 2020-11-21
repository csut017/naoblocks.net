import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-student-connect',
  templateUrl: './student-connect.component.html',
  styleUrls: ['./student-connect.component.scss']
})
export class StudentConnectComponent implements OnInit {

  qrCodeAddress: string;

  constructor() { }

  ngOnInit(): void {
    this.qrCodeAddress = `${environment.apiURL}v1/system/qrcode`;
  }

}
