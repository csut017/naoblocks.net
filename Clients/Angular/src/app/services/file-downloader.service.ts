import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { ClientService } from './client.service';
import { environment } from 'src/environments/environment';
import { saveAs } from 'file-saver';
import { catchError, of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class FileDownloaderService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService,
    private snackBar: MatSnackBar) {
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
      .pipe(
        catchError(error => {
          this.snackBar.open(`Unable to download ${filename}`, undefined, {
            duration: 5000,
            horizontalPosition: 'end',
            verticalPosition: 'bottom'
          });
          return of(null);
        })
      )
      .subscribe((resp: any) => {
        if (!resp) return;
        this.log(`Retrieved file ${filename} from ${path}`);
        saveAs(resp.body, filename);
      });
  }
}  
