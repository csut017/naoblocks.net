import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Robot } from '../data/robot';
import { Observable } from 'rxjs';
import { ExecutionResult } from '../data/execution-result';
import { environment } from 'src/environments/environment';
import { tap, catchError } from 'rxjs/operators';
import { Compilation } from '../data/compilation';

@Injectable({
  providedIn: 'root'
})
export class ProgramService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super('ProgramService', errorHandler);
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
        catchError(this.handleError('compile', msg => new ExecutionResult<Robot>(undefined, msg)))
      );
  }
}
