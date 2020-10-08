import { Injectable } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { catchError, map, tap } from 'rxjs/operators';
import { SystemVersion } from '../data/system-version';
import { ResultSet } from '../data/result-set';
import { SiteAddress } from '../data/site-address';

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

  listRobotAddresses(): Observable<ResultSet<SiteAddress>> {
    const url = `${environment.apiURL}v1/system/addresses`;
    this.log('Retrieving robot client addresses');
    return this.http.get<ResultSet<string>>(url).pipe(
      catchError(this.handleError('listRobotAddresses', msg => new ResultSet<string>(msg))),
      tap(_ => this.log('Robot client addresses retrieved')),
      map(data => {
        let converted = new ResultSet<SiteAddress>(data.errorMsg);
        converted.count = data.count;
        converted.page = data.page;
        converted.items = data.items.map(i => new SiteAddress(i));
        return converted;
      })
    );
  }

  setDefaultAddress(address: string): Observable<ResultSet<any>> {
    const url = `${environment.apiURL}v1/system/siteAddress`;
    this.log('Setting default site address');
    let data = {
      defaultAddress: address
    };
    return this.http.post<ResultSet<any>>(url, data).pipe(
      catchError(this.handleError('setDefaultAddress', msg => new ResultSet<any>(msg))),
      tap(_ => this.log('Set default site address'))
    );
  }
}
