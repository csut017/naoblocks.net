import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';
import { Robot } from '../data/robot';
import { NamedValue } from '../data/named-value';
import { QuickLink } from '../data/quick-link';

@Injectable({
  providedIn: 'root'
})
export class RobotService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'RobotService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<Robot>> {
    const url = `${environment.apiURL}v1/robots?page=${page}&size=${size}`;
    this.log('Listing robots');
    return this.http.get<ResultSet<Robot>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched robots');
          if (data.items) data.items.forEach(s => s.id = s.machineName);
        }),
        catchError(this.handleError('list', msg => new ResultSet<Robot>(msg)))
      );
  }

  listType(robotType: string, page: number = 0, size: number = 20): Observable<ResultSet<Robot>> {
    const url = `${environment.apiURL}v1/robots?page=${page}&size=${size}&type=${robotType}`;
    this.log(`Listing robots of type ${robotType}`);
    return this.http.get<ResultSet<Robot>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched robots');
          if (data.items) data.items.forEach(s => s.id = s.machineName);
        }),
        catchError(this.handleError('listType', msg => new ResultSet<Robot>(msg)))
      );
  }

  get(id: string, includeTypeDetails: boolean = false): Observable<ExecutionResult<Robot>> {
    let url = `${environment.apiURL}v1/robots/${id}`;
    if (includeTypeDetails) url += '?includeType=true';
    this.log(`Retrieving robot ${id}`);
    return this.http.get<Robot>(url)
      .pipe(
        tap(data => {
          data.id = data.machineName;
          this.log(`Retrieved robot ${id}`)
        }),
        map(data => new ExecutionResult<Robot>(data)),
        catchError(this.handleError('get', msg => new ExecutionResult<Robot>(undefined, msg)))
      );
  }

  getQuickLink(id: string, type: string): Observable<ExecutionResult<QuickLink>> {
    const url = `${environment.apiURL}v1/robots/${id}/quicklink/${type}`;
    this.log(`Retrieving ${type} quick link for robot ${id}`);
    return this.http.get<QuickLink>(url)
      .pipe(
        tap(_ => {
          this.log(`Retrieved ${type } quick link for robot ${id}`)
        }),
        map(data => new ExecutionResult<QuickLink>(data)),
        catchError(this.handleError('getQuickLink', msg => new ExecutionResult<QuickLink>(undefined, msg)))
      );
  }

  save(robot: Robot): Observable<ExecutionResult<Robot>> {
    if (robot.isNew) {
      const url = `${environment.apiURL}v1/robots`;
      this.log('Adding new robot');
      return this.http.post<any>(url, robot)
        .pipe(
          tap(_ => this.log('Added new robot')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<Robot>(undefined, msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/robots/${robot.id}`;
      this.log('Updating robot');
      return this.http.put<any>(url, robot)
        .pipe(
          tap(_ => this.log('Updated robot')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<Robot>(undefined, msg)))
        );
    }
  }

  delete(robot: Robot): Observable<ExecutionResult<Robot>> {
    const url = `${environment.apiURL}v1/robots/${robot.id}`;
    this.log('Deleting robot');
    return this.http.delete<ExecutionResult<Robot>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted robot');
          result.output = robot;
        }),
        catchError(this.handleError('delete', msg => new ExecutionResult<Robot>(undefined, msg)))
      );
  }

  parseImportFile(file: File): Observable<ExecutionResult<ResultSet<Robot>>> {
    const url = `${environment.apiURL}v1/robots/import?action=parse`;
    this.log('Parsing import file');
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExecutionResult<ResultSet<Robot>>>(url, formData)
      .pipe(
        tap(result => {
          if (!result.successful) {
            this.log(`Failed to parse input file`);
          } else {
            this.log(`Parsed import file: found ${result.output?.count} robots`);
          }
        }),
        catchError(this.handleError('parseImportFile', msg => new ExecutionResult<ResultSet<Robot>>(new ResultSet<Robot>(), msg))),
      );
  }

  updateValues(robotType: Robot, values: NamedValue[]): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/${robotType.id}/values`;
    this.log(`Updating values for robot ${robotType.id}`);
    const data = {
      items: values,
    };
    return this.http.post<ExecutionResult<any>>(url, data)
      .pipe(
        tap(result => {
          this.log('Updated values');
          result.output = robotType;
        }),
        catchError(this.handleError('updateAllowedValues', msg => new ExecutionResult<Robot>(undefined, msg)))
      );
  }
}
