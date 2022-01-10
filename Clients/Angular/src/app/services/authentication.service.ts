import { Injectable } from '@angular/core';
import { Observable } from 'rxjs'
import { environment } from '../../environments/environment'
import { HttpClient } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';
import { ErrorHandlerService } from './error-handler.service';
import { ClientService } from './client.service';
import { User } from '../data/user';
import { SessionChecker } from '../data/session-checker';
import { ExecutionResult } from '../data/execution-result';
import { Router } from '@angular/router';

export interface LoginResult extends SessionChecker {
  successful: boolean,
  msg: string,
  output?: LoginToken
}

export interface LoginToken {
  role: string,
  token: string,
  view: string
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
  public static readonly tokenKeyName: string = 'authToken';
  public static readonly roleKeyName: string = 'roleName';

  constructor(private http: HttpClient,
    private router: Router,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'AuthenticationService';
    this.token = sessionStorage.getItem(AuthenticationService.tokenKeyName) || '';
    this.role = (<any>UserRole)[sessionStorage.getItem(AuthenticationService.roleKeyName) || 'Unknown'];
  }

  role: UserRole;
  token: string = '';

  login(username: string, password: string, role: string): Observable<LoginResult> {
    const opts = {
      'name': username,
      'password': password,
      'role': role
    };
    return this.doLogin(opts);
  }

  loginViaToken(token: string): Observable<LoginResult>{
    const opts = {
      'token': token
    };
    return this.doLogin(opts);
  }

  private doLogin(opts: any): Observable<LoginResult> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Logging in');
    return this.http.post<LoginResult>(url, opts).pipe(
      catchError(this.handleError('login', this.generateErrorResult)),
      tap(data => {
        if (data.successful && data.output) {
          this.token = data.output.token;
          this.role = (<any>UserRole)[data.output.role];
          sessionStorage.setItem(AuthenticationService.tokenKeyName, this.token);
          sessionStorage.setItem(AuthenticationService.roleKeyName, UserRole[this.role]);
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
    sessionStorage.setItem(AuthenticationService.tokenKeyName, this.token);
  }

  renew(): Observable<LoginResult> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Renewing session');
    return this.http.put<LoginResult>(url, {
    }).pipe(
      catchError(this.handleError('renew', this.generateErrorResult)),
      tap(_ => this.log('Session renewed'))
    );
  }

  getCurrentUser(): Observable<ExecutionResult<User>> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Retrieving current user');
    return this.http.get<User>(url).pipe(
        map(data => new ExecutionResult<User>(data)),
        catchError(this.handleError('login', msg => new ExecutionResult<User>(undefined, msg))),
      tap(_ => this.log('Current user retrieved'))
    );
  }

  private generateErrorResult(msg: string): LoginResult {
    return {
      successful: false,
      msg: msg,
      hasSessionExpired: true
    };
  }

  logout(): Observable<any> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Logging out');
    return this.http.delete(url)
      .pipe(
        catchError(this.handleError('logout', this.generateErrorResult)),
        tap(_ => {
          this.clearCurrentSettings();
        })
      );
  }

  private clearCurrentSettings() {
    this.token = '';
    this.role = UserRole.Unknown;
    sessionStorage.removeItem(AuthenticationService.tokenKeyName);
    sessionStorage.removeItem(AuthenticationService.roleKeyName);
    this.log('Logout complete');
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

  checkHttpResponse(session: SessionChecker): boolean {
    if (!session.hasSessionExpired) return true;

    this.clearCurrentSettings();
    let currentRoute = this.router.url;
    this.router.navigate(['/login'], {
      queryParams: {
        return: currentRoute
      }
    });
  return false;
  }
}
