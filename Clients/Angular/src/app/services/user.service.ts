import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';
import { User } from '../data/user';

@Injectable({
  providedIn: 'root'
})
export class UserService  extends ClientService {

  parseImportFile(file: File): Observable<ExecutionResult<ResultSet<User>>> {
    const url = `${environment.apiURL}v1/students/import?action=parse`;
    this.log('Parsing import file');
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExecutionResult<ResultSet<User>>>(url, formData)
      .pipe(
        tap(result => {
          if (!result.successful) {
            this.log(`Failed to parse input file`);
          } else {
            this.log(`Parsed import file: found ${result.output?.count} students`);
          }
        }),
        catchError(this.handleError('parseImportFile', msg => new ExecutionResult<ResultSet<User>>(new ResultSet<User>(), msg))),
      );
  }

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'UserService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<User>> {
    const url = `${environment.apiURL}v1/users?page=${page}&size=${size}`;
    this.log('Listing users');
    return this.http.get<ResultSet<User>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched users');
          if (data.items) data.items.forEach(s => s.id = s.name);
        }),
        catchError(this.handleError('list', msg => new ResultSet<User>(msg)))
      );
  }

  get(id: string): Observable<ExecutionResult<User>> {
    const url = `${environment.apiURL}v1/users/${id}`;
    this.log(`Retrieving user ${id}`);
    return this.http.get<User>(url)
      .pipe(
        tap(data => {
          data.id = data.name;
          data.isFullyLoaded = true;
          this.log(`Retrieved user ${id}`)
        }),
        map(data => new ExecutionResult<User>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<User>(undefined, msg)))
      );
  }

  save(user: User): Observable<ExecutionResult<User>> {
    if (user.isNew) {
      const url = `${environment.apiURL}v1/users`;
      this.log('Adding new user');
      return this.http.post<any>(url, user)
        .pipe(
          tap(_ => this.log('Added new user')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<User>(undefined, msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/users/${user.id}`;
      this.log('Updating user');
      return this.http.put<any>(url, user)
        .pipe(
          tap(_ => this.log('Updated user')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<User>(undefined, msg)))
        );
    }
  }

  delete(user: User): Observable<ExecutionResult<User>> {
    const url = `${environment.apiURL}v1/users/${user.id}`;
    this.log('Deleting user');
    return this.http.delete<ExecutionResult<User>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted user');
          result.output = user;
        }),
        catchError(this.handleError('saveExisting', msg => new ExecutionResult<User>(undefined, msg)))
      );
  }

  clearLog(user: User): Observable<ExecutionResult<User>> {
    const url = `${environment.apiURL}v1/students/${user.id}/logs`;
    this.log('Deleting user logs');
    return this.http.delete<ExecutionResult<User>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted user logs');
          result.output = user;
        }),
        catchError(this.handleError('clearLog', msg => new ExecutionResult<User>(undefined, msg)))
      );
  }

  clearSnapshot(user: User): Observable<ExecutionResult<User>> {
    const url = `${environment.apiURL}v1/students/${user.id}/snapshots`;
    this.log('Deleting user snapshots');
    return this.http.delete<ExecutionResult<User>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted user snapshots');
          result.output = user;
        }),
        catchError(this.handleError('clearSnapshot', msg => new ExecutionResult<User>(undefined, msg)))
      );
  }
}
