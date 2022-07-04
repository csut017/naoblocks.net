import { Injectable, EventEmitter } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlerService } from './error-handler.service';
import { RobotType } from '../data/robot-type';
import { Observable } from 'rxjs';
import { ResultSet } from '../data/result-set';
import { environment } from 'src/environments/environment';
import { tap, catchError, map } from 'rxjs/operators';
import { ExecutionResult } from '../data/execution-result';
import { PackageFile } from '../data/package-file';
import { BlockSet } from '../data/block-set';
import { RobotTypeBlockDefinitions } from '../data/robot-type-block-definitions';

@Injectable({
  providedIn: 'root'
})
export class RobotTypeService extends ClientService {
  constructor(private http: HttpClient,
    errorHandler: ErrorHandlerService) {
    super(errorHandler);
    this.serviceName = 'RobotTypeService';
  }

  list(page: number = 0, size: number = 20): Observable<ResultSet<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types?page=${page}&size=${size}`;
    this.log('Listing robot types');
    return this.http.get<ResultSet<RobotType>>(url)
      .pipe(
        tap(data => {
          this.log('Fetched robot types');
          if (data.items) data.items.forEach(s => s.id = s.name);
        }),
        catchError(this.handleError('list', msg => new ResultSet<RobotType>(msg)))
      );
  }

  get(id: string): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/${id}`;
    this.log(`Retrieving robot type ${id}`);
    return this.http.get<RobotType>(url)
      .pipe(
        tap(data => {
          data.id = data.name;
          this.log(`Retrieved robot type ${id}`)
        }),
        map(data => new ExecutionResult<RobotType>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  save(robotType: RobotType): Observable<ExecutionResult<RobotType>> {
    if (robotType.isNew) {
      const url = `${environment.apiURL}v1/robots/types`;
      this.log('Adding new robot type');
      return this.http.post<any>(url, robotType)
        .pipe(
          tap(_ => this.log('Added new robot type')),
          catchError(this.handleError('saveNew', msg => new ExecutionResult<RobotType>(new RobotType(), msg)))
        );
    } else {
      const url = `${environment.apiURL}v1/robots/types/${robotType.id}`;
      this.log('Updating robot type');
      return this.http.put<any>(url, robotType)
        .pipe(
          tap(_ => this.log('Updated robot type')),
          catchError(this.handleError('saveExisting', msg => new ExecutionResult<RobotType>(new RobotType(), msg)))
        );
    }
  }

  setSystemDefault(robotType: RobotType): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/default`;
    this.log(`Setting '${robotType.name}' as the system default`);
    return this.http.put<any>(url, robotType)
      .pipe(
        tap(_ => this.log(`Set '${robotType.name}' as the system default`)),
        catchError(this.handleError('setSystemDefault', msg => new ExecutionResult<RobotType>(robotType, msg)))
      );
  }

  delete(robotType: RobotType): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}`;
    this.log('Deleting robot type');
    return this.http.delete<ExecutionResult<RobotType>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted robot type');
          result.output = robotType;
        }),
        catchError(this.handleError('saveExisting', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  storeToolbox(robotType: RobotType, toolbox: string): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/toolbox`;
    this.log(`Storing toolbox for robot type ${robotType.id}`);
    return this.http.post<ExecutionResult<any>>(url, toolbox)
      .pipe(
        tap(result => {
          this.log('Stored toolbox');
          result.output = robotType;
        }),
        catchError(this.handleError('storeToolbox', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  listPackageFiles(robotType: RobotType): Observable<ResultSet<PackageFile>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/package`;
    this.log(`Retrieving package list for robot type ${robotType.id}`);
    return this.http.get<ResultSet<PackageFile>>(url)
      .pipe(
        tap(_ => {
          this.log(`Fetched package list for robot type ${robotType.id}`);
        }),
        catchError(this.handleError('listPackageFiles', msg => new ResultSet<PackageFile>(msg)))
      );
  }

  generatePackageList(robotType: RobotType): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/package`;
    this.log(`Generating package list for robot type ${robotType.id}`);
    return this.http.post<ExecutionResult<any>>(url, {})
      .pipe(
        tap(result => {
          this.log(`Generated package list for robot type ${robotType.id}`);
          result.output = robotType;
        }),
        catchError(this.handleError('generatePackageList', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  uploadPackageFile(robotType: RobotType, filename: string, data: string): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/package/files`;
    this.log(`Uploading package file for robot type ${robotType.id}`);
    const fileData = {
      name: filename,
      value: data
    };
    return this.http.post<ExecutionResult<any>>(url, fileData)
      .pipe(
        tap(result => {
          this.log('Uploaded package file');
          result.output = robotType;
        }),
        catchError(this.handleError('uploadPackageFile', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  listBlockSets(robotTypeId: string, includeBlocks: boolean = false): Observable<RobotTypeBlockDefinitions> {
    if (!robotTypeId) {
      return new Observable(subscriber => {
        subscriber.next(new ResultSet<BlockSet>('Robot type not set'));
      });
    }
    let url = `${environment.apiURL}v1/robots/types/${robotTypeId}/blocksets`;
    if (includeBlocks) url += '?include=blocks';
    this.log(`Retrieving block sets for robot type ${robotTypeId}`);
    return this.http.get<RobotTypeBlockDefinitions>(url)
      .pipe(
        tap(_ => {
          this.log(`Fetched block sets for robot type ${robotTypeId}`);
        }),
        catchError(this.handleError('listBlockSets', msg => new RobotTypeBlockDefinitions(msg)))
      );
  }
}
