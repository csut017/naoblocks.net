import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { ResultSet } from '../data/result-set';
import { Student } from '../data/student';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { tap, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class StudentService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super('StudentService', errorHandler);
  }

  list(page: number = 0): Observable<ResultSet<Student>> {
    const url = `${environment.apiURL}v1/students?page=${page}&size=20`;
    this.log('Listing students');
    return this.http.get<any>(url)
      .pipe(
        tap(_ => this.log('Fetched students')),
        catchError(this.handleError('list', msg => new ResultSet<Student>(msg)))
      );
  }
}
