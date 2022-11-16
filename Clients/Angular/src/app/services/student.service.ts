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

  parseImportFile(file: File): Observable<ExecutionResult<ResultSet<Student>>> {
    const url = `${environment.apiURL}v1/students/import?action=parse`;
    this.log('Parsing import file');
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExecutionResult<ResultSet<Student>>>(url, formData)
      .pipe(
        tap(result => {
          if (!result.successful) {
            this.log(`Failed to parse input file`);
          } else {
            this.log(`Parsed import file: found ${result.output?.count} students`);
          }
        }),
        catchError(this.handleError('parseImportFile', msg => new ExecutionResult<ResultSet<Student>>(new ResultSet<Student>(), msg))),
      );
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
          data.isFullyLoaded = true;
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
        catchError(this.handleError('delete', msg => new ExecutionResult<Student>(undefined, msg)))
      );
  }

  qrCode(student: Student): string {
    const url = `${environment.apiURL}v1/students/${student.id}/qrcode`;
    this.log(`Generated URL for QR code: ${url}`);
    return url;
  }

  clearLog(student: Student): Observable<ExecutionResult<Student>> {
    const url = `${environment.apiURL}v1/students/${student.id}/logs`;
    this.log('Deleting student logs');
    return this.http.delete<ExecutionResult<Student>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted student logs');
          result.output = student;
        }),
        catchError(this.handleError('clearLog', msg => new ExecutionResult<Student>(undefined, msg)))
      );
  }
}
