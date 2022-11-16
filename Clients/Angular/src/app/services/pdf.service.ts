import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export class PdfService extends ClientService {

  pdfMake: any;

  constructor(errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'RobotLogService';
}

  async loadPdfMaker() {
    if (!this.pdfMake) {
      this.log(`Loading pdfMake`);
      const pdfMakeModule = await import('pdfmake/build/pdfmake');
      const pdfFontsModule = await import('pdfmake/build/vfs_fonts');
      this.pdfMake = pdfMakeModule.default;
      this.pdfMake.vfs = pdfFontsModule.default.pdfMake.vfs;
    }
  }

  async generatePdf(def: any, filename: string) {
    await this.loadPdfMaker();
    this.log(`Generating PDF ${filename}`);
    this.pdfMake.createPdf(def).download(filename);
  }

}