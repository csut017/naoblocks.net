import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ExecutionResult } from '../data/execution-result';
import { environment } from 'src/environments/environment';
import { tap, map, catchError } from 'rxjs/operators';
import { UserSettings } from '../data/user-settings';

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'SettingsService';
  }

  get(): Observable<ExecutionResult<UserSettings>> {
    const url = `${environment.apiURL}v1/session/settings`;
    this.log(`Retrieving user settings`);
    return this.http.get<UserSettings>(url)
      .pipe(
        tap(data => {
          data.isLoaded = true;
          this.log(`Retrieved user settings`)
        }),
        map(data => new ExecutionResult<UserSettings>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<UserSettings>(undefined, msg)))
      );
  }

  update(settings: UserSettings): Observable<ExecutionResult<UserSettings>> {
    const url = `${environment.apiURL}v1/session/settings`;
    this.log(`Updating user settings`);
    return this.http.post<ExecutionResult<UserSettings>>(url, settings)
      .pipe(
        tap(_ => {
          this.log(`Updated user settings`)
        }),
        catchError(this.handleError('list', msg => new ExecutionResult<UserSettings>(undefined, msg)))
      );
  }
}
