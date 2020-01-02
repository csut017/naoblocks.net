import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export abstract class ClientService {

  constructor(private serviceName: string,
    protected errorhandler: ErrorHandlerService) { }

  protected log(message: string, data?: any) {
    const msg = `[${this.serviceName}] ${message}`;
    console.log(msg);
  }

  protected logData(message: string, data: any) {
    const msg = `[${this.serviceName}] ${message}`;
    console.groupCollapsed(msg);
    console.log(data);
    console.groupEnd();
  }

  protected error(message: string) {
    console.error(`[${this.serviceName}] ${message}`);
  }

  protected handleError<T>(operation: string, generator: (msg: string) => T) {
    return (error: any): Observable<T> => {
      let msg = '';
      if (error.status === 401) {
        msg = 'Unauthorized';
      } else {
        msg = this.errorhandler.formatError(error);
      }
      this.log(`${operation} failed: ${msg}`);
      return of(generator(msg));
    };
  }
}
