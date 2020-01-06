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
    super(errorHandler);
    this.serviceName = 'StudentService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<Student>> {
    const url = `${environment.apiURL}v1/students?page=${page}&size=${size}`;
    this.log('Listing students');
    return this.http.get<ResultSet<Student>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched students');
          if (data.items) data.items.forEach(s => s.id = s.name);
        }),
        catchError(this.handleError('list', msg => new ResultSet<Student>(msg)))
      );
  }

  get(id: string): Observable<ExecutionResult<Student>> {
    const url = `${environment.apiURL}v1/students/${id}`;
    this.log(`Retrieving student ${id}`);
    return this.http.get<Student>(url)
      .pipe(
        tap(data => {
          data.id = data.name;
          this.log(`Retrieved student ${id}`)
        }),
        map(data => new ExecutionResult<Student>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<Student>(undefined, msg)))
      );
  }

  save(student: Student): Observable<ExecutionResult<Student>> {
    if (student.isNew) {
      const url = `${environment.apiURL}v1/students`;
      this.log('Adding new student');
      return this.http.post<any>(url, student)
        .pipe(
          tap(_ => this.log('Added new student')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<Student>(undefined, msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/students/${student.id}`;
      this.log('Updating student');
      return this.http.put<any>(url, student)
        .pipe(
          tap(_ => this.log('Updated student')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<Student>(undefined, msg)))
        );
    }
  }

  delete(student: Student): Observable<ExecutionResult<Student>> {
    const url = `${environment.apiURL}v1/students/${student.id}`;
    this.log('Deleting student');
    return this.http.delete<ExecutionResult<Student>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted student');
          result.output = student;
        }),
        catchError(this.handleError('saveExisting', msg => new ExecutionResult<Student>(undefined, msg)))
      );
  }
}
