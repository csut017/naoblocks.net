import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';
import { PdfService } from '../services/pdf.service';

@Component({
  selector: 'app-student-connect',
  templateUrl: './student-connect.component.html',
  styleUrls: ['./student-connect.component.scss']
})
export class StudentConnectComponent implements OnInit {

  qrCodeAddress: string;
  generating: boolean = false;
  imageData: string;

  constructor(private pdfService: PdfService) { }

  ngOnInit(): void {
    this.qrCodeAddress = `${environment.apiURL}v1/system/qrcode`;
  }

  async download() {
    this.generating = true;
    if (!this.imageData) {
      let imgToExport = document.getElementById('qrCode') as HTMLImageElement;
      let canvas = document.createElement('canvas');
      canvas.width = imgToExport.width;
      canvas.height = imgToExport.height;
      canvas.getContext('2d').drawImage(imgToExport, 0, 0);
      this.imageData = canvas.toDataURL('image/png');
    }

    const def = {
      pageSize: 'A5',
      content: [
        {
          text: 'NaoBlocks',
          bold: true
        },
        {
          image: this.imageData,
          width: 300,
          style: 'centre'
        }
      ],
      styles: {
        centre: {
          alignment: 'center'
        }
      }
    };
    await this.pdfService.generatePdf(def, 'NaoBlocks-QR.pdf');
    this.generating = false;
  }

}
