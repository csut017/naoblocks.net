import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export abstract class ClientService {

  constructor(private serviceName: string,
    protected errorhandler: ErrorHandlerService) { }

  protected log(message: string) {
    console.log(`[${this.serviceName}] ${message}`);
  }

  protected handleError<T>(operation: string, generator: (msg: string) => T) {
    return (error: any): Observable<T> => {
      const msg = this.errorhandler.formatError(error);
      this.log(`${operation} failed: ${msg}`);
      return of(generator(msg));
    };
  }
}
