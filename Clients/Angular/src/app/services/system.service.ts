import { Injectable } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { forkJoin, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { catchError, map, tap } from 'rxjs/operators';
import { SystemVersion } from '../data/system-version';
import { ResultSet } from '../data/result-set';
import { SiteAddress } from '../data/site-address';
import { SiteConfiguration } from '../data/site-configuration';

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
    const addressesUrl = `${environment.apiURL}v1/system/addresses`;
    const configUrl = `${environment.apiURL}v1/system/config`;
    this.log('Retrieving robot client addresses');
    return forkJoin([this.http.get<ResultSet<string>>(addressesUrl).pipe(
      catchError(this.handleError('listRobotAddresses', msg => new ResultSet<string>(msg))),
      tap(_ => this.log('Robot client addresses retrieved'))
    ), this.http.get<SiteConfiguration>(configUrl).pipe(
      catchError(this.handleError('listRobotAddresses', _ => new SiteConfiguration())),
      tap(_ => this.log('Site configuration retrieved'))
    )]).pipe(
      map(result => {
        let data = result[0];
        let converted = new ResultSet<SiteAddress>(data.errorMsg);
        converted.count = data.count;
        converted.page = data.page;
        converted.items = data.items.map(i => new SiteAddress(i));

        let config = result[1];
        converted.items.forEach(a => a.isDefault = a.url === config.defaultAddress);
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
      tap(_ => this.log('Set default site address response received'))
    );
  }

  initialise(adminPassword: string, useDefaultUi: boolean, addNaoRobot: boolean): Observable<SystemVersion> {
    const url = `${environment.apiURL}v1/system/initialise`;
    this.log('Initialising application');
    let data = {
      useDefaultUi: useDefaultUi,
      password: adminPassword,
      addNaoRobot: addNaoRobot
    };
    return this.http.post<SystemVersion>(url, data).pipe(
      catchError(this.handleError('initialise', msg => new SystemVersion(msg))),
      tap(_ => this.log('System initialisation response received'))
    );
  }
}
