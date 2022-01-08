import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ExecutionResult } from '../data/execution-result';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { Compilation } from '../data/compilation';
import { ResultSet } from '../data/result-set';
import { ProgramFile } from '../data/program-file';

@Injectable({
  providedIn: 'root'
})
export class ProgramService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
      super(errorHandler);
      this.serviceName = 'ProgramService';
  }

  compile(code: string, store: boolean = true): Observable<ExecutionResult<Compilation>> {
    const url = `${environment.apiURL}v1/code/compile`;
    this.log('Compiling code');
    let request = {
      code: code,
      store: store
    };
    return this.http.post<any>(url, request)
      .pipe(
        tap(_ => this.log('Code compiled')),
        catchError(this.handleError('compile', msg => new ExecutionResult<Compilation>(undefined, msg)))
      );
  }

  get(userName: string, id: string): Observable<ExecutionResult<ProgramFile>> {
    const url = `${environment.apiURL}v1/programs/${id}?user=${userName}`;
    this.log(`Retrieving program ${id}`);
    return this.http.get<ProgramFile>(url)
      .pipe(
        tap(data => {
          this.log(`Retrieved program ${id}`)
        }),
        map(data => new ExecutionResult<ProgramFile>(data)),
        catchError(this.handleError('get', msg => new ExecutionResult<ProgramFile>(undefined, msg)))
      );
  }

  getAST(userName: string, id: string): Observable<ExecutionResult<Compilation>> {
    const url = `${environment.apiURL}v1/code/${userName}/${id}`;
    this.log(`Retrieving program AST ${id}`);
    return this.http.get<ExecutionResult<Compilation>>(url)
      .pipe(
        tap(_ => {
          this.log(`Retrieved program AST ${id}`)
        }),
        catchError(this.handleError('getAST', msg => new ExecutionResult<Compilation>(undefined, msg)))
      );
  }

  list(userName: string, page: number = 0, size: number = 20): Observable<ResultSet<ProgramFile>> {
    const url = `${environment.apiURL}v1/programs?page=${page}&size=${size}&user=${userName}`;
    this.log('Listing programs');
    return this.http.get<ResultSet<ProgramFile>>(url)
      .pipe(
        tap(_ => {
          this.log('Fetched programs');
        }),
        catchError(this.handleError('list', msg => new ResultSet<ProgramFile>(msg)))
      );
  }

  save(name: string, code: string): Observable<ExecutionResult<ProgramFile>> {
    const url = `${environment.apiURL}v1/programs`;
    this.log('Storing program');
    let request = {
      name: name,
      code: code
    };
    return this.http.post<any>(url, request)
      .pipe(
        tap(_ => this.log('Stored program')),
        catchError(this.handleError('save', msg => new ExecutionResult<ProgramFile>(undefined, msg)))
      );
  }
}
