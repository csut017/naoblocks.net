import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap, catchError } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResultSet } from '../data/result-set';
import { UIDefinition } from '../data/ui-definition';
import { ClientService } from './client.service';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export class UiService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'UiService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<UIDefinition>> {
    const url = `${environment.apiURL}v1/ui?page=${page}&size=${size}`;
    this.log('Listing robots');
    return this.http.get<ResultSet<UIDefinition>>(url)
      .pipe(
        tap(_ => this.log('Fetched definitions')),
        catchError(this.handleError('list', msg => new ResultSet<UIDefinition>(msg)))
      );
  }
}
