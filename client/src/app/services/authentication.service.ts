import { Injectable, ErrorHandler } from '@angular/core';
import { Observable, of } from 'rxjs'
import { environment } from '../../environments/environment'
import { HttpClient } from '@angular/common/http';
import { catchError, tap } from 'rxjs/operators';
import { ErrorHandlerService } from './error-handler.service';
import { ClientService } from './client.service';
import { User } from '../data/user';

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
export class AuthenticationService extends ClientService {
  private tokenKeyName: string = 'authToken';
  private roleKeyName: string = 'roleName';

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'AuthenticationService';
    this.token = sessionStorage.getItem(this.tokenKeyName) || '';
    this.role = UserRole[sessionStorage.getItem(this.roleKeyName) || 'Unknown'];
  }

  role: UserRole;
  token: string = '';

  login(username: string, password: string, role: string): Observable<login> {
    const opts = {
      'name': username,
      'password': password,
      'role': role
    };
    return this.doLogin(opts);
  }

  loginViaToken(token: string): Observable<login>{
    const opts = {
      'token': token
    };
    return this.doLogin(opts);
  }

  private doLogin(opts: any) {
    const url = `${environment.apiURL}v1/session`;
    this.log('Logging in');
    return this.http.post<login>(url, opts).pipe(
      catchError(this.handleError('login', this.generateErrorResult)),
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

  resume(session: string) {
    this.token = session;
    sessionStorage.setItem(this.tokenKeyName, this.token);
  }

  renew(): Observable<login> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Renewing session');
    return this.http.put<login>(url, {
    }).pipe(
      catchError(this.handleError('renew', this.generateErrorResult)),
      tap(_ => this.log('Session renewed'))
    );
  }

  getCurrentUser(): Observable<User> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Retrieving current user');
    return this.http.get<User>(url).pipe(
      catchError(this.handleError('login', _ => null)),
      tap(_ => this.log('Current user retrieved'))
    );
  }

  private generateErrorResult(msg: string): login {
    return {
      successful: false,
      msg: msg
    };
  }

  logout(): Observable<any> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Logging out');
    return this.http.delete(url)
      .pipe(
        catchError(this.handleError('logout', this.generateErrorResult)),
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
}
