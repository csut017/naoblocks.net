import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs'
import { environment } from '../../environments/environment'
import { HttpClient } from '@angular/common/http';
import { catchError, shareReplay, tap, share } from 'rxjs/operators';
import { ErrorHandlerService } from './error-handler.service';

interface login {
  successful: boolean,
  msg: string,
  output?: loginToken
}

interface loginToken {
  role: string,
  token: string
}

export enum UserRole {
  Unknown = 0,
  Student,
  Teacher,
  Administrator
}

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  private tokenKeyName: string = 'authToken';
  private roleKeyName: string = 'roleName';

  constructor(private http: HttpClient,
    private errorhandler: ErrorHandlerService) {
    this.token = sessionStorage.getItem(this.tokenKeyName) || '';
    this.role = UserRole[sessionStorage.getItem(this.roleKeyName) || 'Unknown'];
  }

  role: UserRole;
  token: string = '';

  login(username: string, password: string, role: string): Observable<login> {
    const url = `${environment.apiURL}v1/session`;
    return this.http.post<login>(url, {
      'name': username,
      'password': password,
      'role': role
    }).pipe(
      share(),
      catchError(this.handleError('login')),
      tap(data => {
        if (data.successful && data.output) {
          this.token = data.output.token;
          this.role = UserRole[data.output.role];
          sessionStorage.setItem(this.tokenKeyName, this.token);
          sessionStorage.setItem(this.roleKeyName, UserRole[this.role]);
        } else {
          this.token = '';
          this.role = UserRole.Unknown;
        }
        this.log('Login complete')
      })
    );
  }

  logout(): Observable<any> {
    const url = `${environment.apiURL}v1/session`;
    return this.http.delete(url)
      .pipe(
        catchError(this.handleError('logout')),
        tap(_ => {
          this.token = '';
          this.role = UserRole.Unknown;
          sessionStorage.removeItem(this.tokenKeyName);
          sessionStorage.removeItem(this.roleKeyName);
          this.log('Logout complete')
        })
      );
  }

  isCurrentRole(role: UserRole): boolean {
    return this.role == role;
  }

  isValid(): boolean {
    return !!this.token;
  }

  canAccess(role: UserRole): boolean {
    return this.role >= role;
  }

  private log(message: string) {
    console.log(`AuthenticationService: ${message}`);
  }

  private handleError(operation: string) {
    return (error: any): Observable<login> => {
      const msg = this.errorhandler.formatError(error);
      this.log(`${operation} failed: ${msg}`);
      return of({
        successful: false,
        msg: msg
      });
    };
  }
}
