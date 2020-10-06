import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { ExecutionResult } from '../data/execution-result';
import { Snapshot } from '../data/snapshot';
import { SnapshotValue } from '../data/snapshot-value';
import { ClientService } from './client.service';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export class SnapshotService extends ClientService {

  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'SnapshotService';
  }

  store(state: string, source: string, values: { [index: string]: string }): Observable<ExecutionResult<Snapshot>> {
    const url = `${environment.apiURL}v1/snapshots`;
    this.log('Storing snapshot');
    let snapshot = new Snapshot();
    snapshot.state = state;
    snapshot.source = source;
    snapshot.values = [];

    Object.entries(values).forEach(
      ([key, value]) => {
        let val = new SnapshotValue();
        val.name = key;
        val.value = value;
        snapshot.values.push(val);
      });

    return this.http.post<any>(url, snapshot)
      .pipe(
        tap(_ => this.log('Stored snapshot')),
        catchError(this.handleError('store', msg => new ExecutionResult<Snapshot>(undefined, msg)))
      );
  }
}
