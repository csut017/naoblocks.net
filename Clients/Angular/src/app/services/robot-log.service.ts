import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';
import { RobotLog } from '../data/robot-log';

@Injectable({
  providedIn: 'root'
})
export class RobotLogService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'RobotLogService';
}

list(robot:string, page: number = 0, size: number = 20): Observable<ResultSet<RobotLog>> {
  const url = `${environment.apiURL}v1/robots/${robot}/logs?page=${page}&size=${size}`;
  this.log(`Listing robot logs for ${robot}`);
  return this.http.get<ResultSet<RobotLog>>(url)
    .pipe(
      tap(_ => {
        this.log('Fetched robot logs');
      }),
      catchError(this.handleError('list', msg => new ResultSet<RobotLog>(msg)))
    );
}

get(robot:string, id: number): Observable<ExecutionResult<RobotLog>> {
  const url = `${environment.apiURL}v1/robots/${robot}/logs/${id}`;
  this.log(`Retrieving robot log ${id} for ${robot}`);
  return this.http.get<RobotLog>(url)
    .pipe(
      tap(_ => {
        this.log(`Retrieved robot log ${id}`)
      }),
      map(data => new ExecutionResult<RobotLog>(data)),
      catchError(this.handleError('list', msg => new ExecutionResult<RobotLog>(undefined, msg)))
    );
}
}
