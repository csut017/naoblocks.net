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
  token: string
}

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  private keyName: string = 'authToken';

  constructor(private http: HttpClient,
    private errorhandler: ErrorHandlerService) {
      this.token = sessionStorage.getItem(this.keyName) || '';
     }

  token: string = '';

  login(username: string, password: string, role: string): Observable<login> {
    const url = `${environment.apiURL}v1/session`;
    return this.http.post<login>(url, {
      'name': username,
      'password': password,
      'role': role
    }).pipe(
      share(),
      catchError(this.handleError()),
      tap(data => {
        if (data.successful && data.output) {
          this.token = data.output.token;
          sessionStorage.setItem(this.keyName, this.token);
        } else {
          this.token = '';
        }
        this.log('Login complete')
      })
    );
  }

  logout(): Observable<any> {
    const url = `${environment.apiURL}v1/session`;
    return this.http.delete(url)
    .pipe(
      tap(_ => {
        this.token = '';
        sessionStorage.removeItem(this.keyName);
        this.log('Logout complete')
      })
    );
  }

  isValid(): boolean {
    return !!this.token;
  }

  private log(message: string) {
    console.log(`AuthenticationService: ${message}`);
  }

  private handleError() {
    return (error: any): Observable<login> => {
      const msg = this.errorhandler.formatError(error);
      this.log(`login failed: ${msg}`);
      return of({
        successful: false,
        msg: msg
      });
    };
  }
}
