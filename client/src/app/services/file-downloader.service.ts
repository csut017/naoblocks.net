import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { ClientService } from './client.service';
import { environment } from 'src/environments/environment';
import { saveAs } from 'file-saver';

@Injectable({
  providedIn: 'root'
})
export class FileDownloaderService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'fileDownloaderService';
  }

  download(path: string, filename?: string): void {
    const url = `${environment.apiURL}${path}`;
    this.log(`Downloading file from ${path}`);
    this.http.get<Blob>(url, {
      responseType: 'blob' as 'json',
      observe: 'response'
    })
      .subscribe((resp: any) => {
        this.log(`Retrieved file ${filename} from ${path}`);
        saveAs(resp.body, filename);
      });
  }
}  
