import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Robot } from '../data/robot';
import { RobotType } from '../data/robot-type';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';

@Injectable({
  providedIn: 'root'
})
export class RobotTypeService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'RobotTypeService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<RobotType>> {
    const url = `${environment.apiURL}v1/robots?page=${page}&size=${size}`;
    this.log('Listing robots');
    return this.http.get<ResultSet<RobotType>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched robot types');
          if (data.items) data.items.forEach(s => s.id = s.name);
        }),
        catchError(this.handleError('list', msg => new ResultSet<RobotType>(msg)))
      );
  }

  get(id: string): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/${id}`;
    this.log(`Retrieving robot ${id}`);
    return this.http.get<RobotType>(url)
      .pipe(
        tap(data => {
          data.id = data.name;
          this.log(`Retrieved robot type ${id}`)
        }),
        map(data => new ExecutionResult<RobotType>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  save(robotType: RobotType): Observable<ExecutionResult<RobotType>> {
    if (robotType.isNew) {
      const url = `${environment.apiURL}v1/robots`;
      this.log('Adding new robot type');
      return this.http.post<any>(url, robotType)
        .pipe(
          tap(_ => this.log('Added new robot type')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<Robot>(undefined, msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/robots/${robotType.id}`;
      this.log('Updating robot type');
      return this.http.put<any>(url, robotType)
        .pipe(
          tap(_ => this.log('Updated robot type')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<Robot>(undefined, msg)))
        );
    }
  }

  delete(robotType: RobotType): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/${robotType.id}`;
    this.log('Deleting robot type');
    return this.http.delete<ExecutionResult<RobotType>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted robot type');
          result.output = robotType;
        }),
        catchError(this.handleError('saveExisting', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }
}
