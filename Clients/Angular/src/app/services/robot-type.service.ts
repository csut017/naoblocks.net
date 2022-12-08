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
import { Toolbox } from '../data/toolbox';
import { NamedValue } from '../data/named-value';

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

  getToolbox(id: string, name: string, format: string = 'toolbox'): Observable<ExecutionResult<Toolbox>> {
    const url = `${environment.apiURL}v1/robots/types/${id}/toolbox/${name}?format=${format}`;
    this.log(`Retrieving toolbox ${name} for robot type ${id}`);
    return this.http.get<Toolbox>(url)
      .pipe(
        tap(data => {
          data.id = data.name;
          this.log(`Retrieved toolbox ${name} for robot type ${id}`)
        }),
        map(data => new ExecutionResult<Toolbox>(data)),
        catchError(this.handleError('list', msg => new ExecutionResult<Toolbox>(undefined, msg)))
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
        catchError(this.handleError('delete', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  deleteToolbox(robotType: RobotType, toolbox: string): Observable<ExecutionResult<Toolbox>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/toolbox/${toolbox}`;
    this.log(`Deleting toolbox ${toolbox} from ${robotType.name}`);
    return this.http.delete<ExecutionResult<Toolbox>>(url)
      .pipe(
        tap(result => {
          this.log('Deleted toolbox');
          result.output = result.output;
        }),
        catchError(this.handleError('deleteToolbox', msg => new ExecutionResult<Toolbox>(undefined, msg)))
      );
  }

  importToolbox(robotType: RobotType, name: string, definition: string, isDefault: boolean, useEvents: boolean): Observable<ExecutionResult<Toolbox>> {
    const defaultOption = isDefault ? 'yes' : 'no',
          eventsOption = useEvents ? 'yes' : 'no';
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/toolbox/${name}?default=${defaultOption}&events=${eventsOption}`;
    this.log(`Importing toolbox ${name} to ${robotType.name}`);
    return this.http.post<ExecutionResult<Toolbox>>(url, definition)
      .pipe(
        tap(result => {
          this.log('Stored toolbox');
          result.output = robotType.toolboxes?.find(t => t.name == name);
        }),
        catchError(this.handleError('storeToolbox', msg => new ExecutionResult<Toolbox>(undefined, msg)))
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
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/package/${filename}`;
    this.log(`Uploading package file for robot type ${robotType.id}`);
    return this.http.post<ExecutionResult<any>>(url, data)
      .pipe(
        tap(result => {
          this.log('Uploaded package file');
          result.output = robotType;
        }),
        catchError(this.handleError('uploadPackageFile', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  uploadBlockSet(robotType: RobotType, name: string, definition: string): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/blocksets`;
    this.log(`Adding blockset for robot type ${robotType.id}`);
    const fileData = {
      name: name,
      value: definition
    };
    return this.http.post<ExecutionResult<any>>(url, fileData)
      .pipe(
        tap(result => {
          this.log('Uploaded blockset');
          result.output = robotType;
        }),
        catchError(this.handleError('uploadBlockSet', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }

  parseImportFile(file: File): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/import?action=parse`;
    this.log('Parsing import file');
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ExecutionResult<RobotType>>(url, formData)
      .pipe(
        tap(result => {
          if (!result.successful) {
            this.log(`Failed to parse input file`);
          } else {
            this.log(`Parsed import file: found definition for ${result.output?.name}`);
          }
        }),
        catchError(this.handleError('parseImportFile', msg => new ExecutionResult<RobotType>(new RobotType(), msg))),
      );
  }

  updateAllowedValues(robotType: RobotType, values: NamedValue[]): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/values`;
    this.log(`Updating values for robot type ${robotType.id}`);
    const data = {
      items: values,
    };
    return this.http.post<ExecutionResult<any>>(url, data)
      .pipe(
        tap(result => {
          this.log('Updated values');
          result.output = robotType;
        }),
        catchError(this.handleError('updateAllowedValues', msg => new ExecutionResult<RobotType>(undefined, msg)))
      );
  }
}
