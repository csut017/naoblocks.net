import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {

  constructor() { }

  formatError(data: any): string {
    if (data.error) {
      if (data.error.validationErrors) return data.error.validationErrors.map(e => e.error || e).join()
      if (data.error.executionErrors) return data.error.executionErrors.map(e => e.error || e).join()
    }
    if (data.validationErrors) return data.validationErrors.map(e => e.error || e).join()
    if (data.executionErrors) return data.executionErrors.map(e => e.error || e).join()
    return data.message;
  }
}
