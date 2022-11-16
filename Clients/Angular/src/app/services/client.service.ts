import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { SessionChecker } from '../data/session-checker';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export abstract class ClientService {

  serviceName: string = '<unknown>';

  constructor(protected errorhandler: ErrorHandlerService) { }

  protected log(message: string) {
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

  protected handleError<T extends SessionChecker>(operation: string, generator: (msg: string) => T) {
    return (error: any): Observable<T> => {
      let msg = '';
      let isSessionValid = true;
      switch (error.status) {
        case 401:
          msg = 'Unauthorized';
          isSessionValid = false;
          break;
        case 0:
          msg = 'Connection lost';
          break;
        default:
          msg = this.errorhandler.formatError(error);
          break;
      }

      this.log(`${operation} failed: ${msg}`);
      let entity = generator(msg);
      entity.hasSessionExpired = !isSessionValid;
      return of(entity);
    };
  }
}
