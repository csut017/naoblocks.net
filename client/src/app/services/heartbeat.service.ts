import { Injectable, ErrorHandler } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { ClientService } from './client.service';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { catchError, share, tap } from 'rxjs/operators';

export class heartbeat {
  timeRemaining: number;
  error: string;

  constructor(error?: string) {
    this.error = error;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HeartbeatService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super('heartbeatService', errorHandler);
  }

  check(): Observable<heartbeat> {
    const url = `${environment.apiURL}v1/session`;
    this.log('Checking heartbeat');
    return this.http.get<heartbeat>(url).pipe(
      catchError(this.handleError('login', msg => new heartbeat(msg))),
      tap(_ => this.log('Heartbeat returned'))
    );
  }
}