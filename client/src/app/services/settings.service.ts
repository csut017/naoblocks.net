import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ExecutionResult } from '../data/execution-result';
import { environment } from 'src/environments/environment';
import { tap, map, catchError } from 'rxjs/operators';
import { EditorSettings } from '../data/editor-settings';

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'SettingsService';
  }

  get(): Observable<ExecutionResult<EditorSettings>> {
    const url = `${environment.apiURL}v1/session/settings`;
    this.log(`Retrieving editor settings`);
    return this.http.get<EditorSettings>(url)
      .pipe(
        tap(data => {
          data.isLoaded = true;
          this.log(`Retrieved editor settings`)
        }),
        map(data => new ExecutionResult<EditorSettings>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<EditorSettings>(undefined, msg)))
      );
  }

  update(settings: EditorSettings): Observable<ExecutionResult<EditorSettings>> {
    const url = `${environment.apiURL}v1/session/settings`;
    this.log(`Updating editor settings`);
    return this.http.post<ExecutionResult<EditorSettings>>(url, settings)
      .pipe(
        tap(_ => {
          this.log(`Updated editor settings`)
        }),
        catchError(this.handleError('update', msg => new ExecutionResult<EditorSettings>(undefined, msg)))
      );
  }
}
