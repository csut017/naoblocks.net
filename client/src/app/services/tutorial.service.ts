import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { Tutorial } from '../data/tutorial';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';

@Injectable({
  providedIn: 'root'
})
export class TutorialService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'TutorialService';
  }

  list(category?: string, page: number = 0, size: number = 20): Observable<ResultSet<Tutorial>> {
    const url = category
      ? `${environment.apiURL}v1/tutorials/${category}?page=${page}&size=${size}`
      : `${environment.apiURL}v1/tutorials?page=${page}&size=${size}`;
    this.log('Listing tutorials');
    return this.http.get<ResultSet<Tutorial>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched tutorials');
          if (data.items) data.items.forEach(t => t.id = `${t.category}/${t.name}`);
        }),
        catchError(this.handleError('list', msg => new ResultSet<Tutorial>(msg)))
      );
  }

  get(id: string): Observable<ExecutionResult<Tutorial>> {
    const url = `${environment.apiURL}v1/tutorials/${id}`;
    this.log(`Retrieving tutorial ${id}`);
    return this.http.get<Tutorial>(url)
      .pipe(
        tap(data => {
          data.id = `${data.category}/${data.name}`;
          this.log(`Retrieved tutorial ${id}`)
        }),
        map(data => new ExecutionResult<Tutorial>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<Tutorial>(undefined, msg)))
      );
  }

  save(tutorial: Tutorial): Observable<ExecutionResult<Tutorial>> {
    if (tutorial.isNew) {
      const url = `${environment.apiURL}v1/tutorials`;
      this.log('Adding new tutorial');
      return this.http.post<any>(url, tutorial)
        .pipe(
          tap(_ => this.log('Added new tutorial')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<Tutorial>(undefined, msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/tutorials/${tutorial.id}`;
      this.log('Updating tutorial');
      return this.http.put<any>(url, tutorial)
        .pipe(
          tap(_ => this.log('Updated tutorial')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<Tutorial>(undefined, msg)))
        );
    }
  }

  delete(tutorial: Tutorial): Observable<ExecutionResult<Tutorial>> {
    const url = `${environment.apiURL}v1/tutorials/${tutorial.id}`;
    this.log('Deleting tutorial');
    return this.http.delete<ExecutionResult<Tutorial>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted tutorial');
          result.output = tutorial;
        }),
        catchError(this.handleError('saveExisting', msg => new ExecutionResult<Tutorial>(undefined, msg)))
      );
  }
}
