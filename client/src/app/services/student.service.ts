import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { ResultSet } from '../data/result-set';
import { Student } from '../data/student';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';

@Injectable({
  providedIn: 'root'
})
export class StudentService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super('StudentService', errorHandler);
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<Student>> {
    const url = `${environment.apiURL}v1/students?page=${page}&size=${size}`;
    this.log('Listing students');
    return this.http.get<any>(url)
      .pipe(
        tap(_ => this.log('Fetched students')),
        catchError(this.handleError('list', msg => new ResultSet<Student>(msg)))
      );
  }

  save(student: Student): Observable<ExecutionResult<Student>> {
    if (student.isNew) {
      const url = `${environment.apiURL}v1/students`;
      this.log('Adding new student');
      return this.http.post<any>(url, student)
        .pipe(
          tap(_ => this.log('Added new student')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<Student>(msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/students/${student.name}`;
      this.log('Updating student');
      return this.http.put<any>(url, student)
        .pipe(
          tap(_ => this.log('Updated student')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<Student>(msg)))
        );
    }
  }
}
