import { Injectable } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { catchError, tap } from 'rxjs/operators';
import { SystemVersion } from '../data/system-version';

@Injectable({
  providedIn: 'root'
})
export class SystemService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'SystemService';
}

  refresh(): Observable<SystemStatus> {
    const url = `${environment.apiURL}v1/system/status`;
    this.log('Retrieving system status');
    return this.http.get<SystemStatus>(url).pipe(
      catchError(this.handleError('login', msg => new SystemStatus(msg))),
      tap(_ => this.log('System status retrieved'))
    );
  }

  getVersion(): Observable<SystemVersion> {
    const url = `${environment.apiURL}v1/version`;
    this.log('Retrieving system version');
    return this.http.get<SystemVersion>(url).pipe(
      catchError(this.handleError('login', msg => new SystemVersion(msg))),
      tap(_ => this.log('System version retrieved'))
    );
  }
}
