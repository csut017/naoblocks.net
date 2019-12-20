import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {

  constructor() { }

  formatError(data: any): string {
    if (data.error) {
      if (data.error.validationErrors) return data.error.validationErrors.join()
      if (data.error.executionErrors) return data.error.executionErrors.join()
    }
    return data.message;
  }
}
