import { Injectable, EventEmitter } from '@angular/core';
import { ClientService } from './client.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
import { LoggingTemplate } from '../data/logging-template';
import { RobotTypeItems } from '../data/robot-type-items';

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
        catchError(this.handleError('list', error => new ResultSet<RobotType>(error)))
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
        catchError(this.handleError('list', error => new ExecutionResult<RobotType>(undefined, error)))
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
        catchError(this.handleError('list', error => new ExecutionResult<Toolbox>(undefined, error)))
      );
  }

  save(robotType: RobotType, message?: string): Observable<ExecutionResult<RobotType>> {
    if (robotType.isNew) {
      const url = `${environment.apiURL}v1/robots/types`;
      this.log('Adding new robot type');
      return this.http.post<any>(url, robotType)
        .pipe(
          tap(res => {
            this.log('Added new robot type');
            res.message = message;
          }),
          catchError(this.handleError('saveNew', error => new ExecutionResult<RobotType>(new RobotType(), error, message)))
        );
    } else {
      const url = `${environment.apiURL}v1/robots/types/${robotType.id}`;
      this.log('Updating robot type');
      return this.http.put<any>(url, robotType)
        .pipe(
          tap(res => {
            this.log('Updated robot type');
            res.message = message;
          }),
          catchError(this.handleError('saveExisting', error => new ExecutionResult<RobotType>(new RobotType(), error, message)))
        );
    }
  }

  setSystemDefault(robotType: RobotType, message?: string): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/default`;
    this.log(`Setting '${robotType.name}' as the system default`);
    return this.http.put<ExecutionResult<RobotType>>(url, robotType)
      .pipe(
        tap(res => {
          this.log(`Set '${robotType.name}' as the system default`);
          res.message = message;
        }),
        catchError(this.handleError('setSystemDefault', error => new ExecutionResult<RobotType>(robotType, error, message)))
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
        catchError(this.handleError('delete', error => new ExecutionResult<RobotType>(undefined, error)))
      );
  }

  clearData(robotType: RobotType, items: RobotTypeItems, message?: string): Observable<ExecutionResult<RobotType>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/data`;
    this.log('Clearing robot type data');
    let itemsToWipe: string[] = [];
    if (items.toolboxes) itemsToWipe.push('toolboxes');
    if (items.values) itemsToWipe.push('customValues');
    if (items.templates) itemsToWipe.push('loggingTemplates');
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
      body: {
        items: itemsToWipe,
      },
    };
    return this.http.delete<ExecutionResult<RobotType>>(url, options)
      .pipe(
        tap(result => {
          this.log('Deleted robot type');
          result.output = robotType;
          result.message = message;
        }),
        catchError(this.handleError('delete', error => new ExecutionResult<RobotType>(undefined, error, message)))
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
        catchError(this.handleError('deleteToolbox', error => new ExecutionResult<Toolbox>(undefined, error)))
      );
  }

  importToolbox(robotType: RobotType, name: string, definition: string, isDefault: boolean, useEvents: boolean, message?: string): Observable<ExecutionResult<Toolbox>> {
    const defaultOption = isDefault ? 'yes' : 'no',
      eventsOption = useEvents ? 'yes' : 'no';
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/toolbox/${name}?default=${defaultOption}&events=${eventsOption}`;
    this.log(`Importing toolbox ${name} to ${robotType.name}`);
    return this.http.post<ExecutionResult<Toolbox>>(url, definition)
      .pipe(
        tap(result => {
          this.log('Stored toolbox');
          result.message = message;
          result.output = robotType.toolboxes?.find(t => t.name == name);
        }),
        catchError(this.handleError('storeToolbox', error => new ExecutionResult<Toolbox>(undefined, error, message)))
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
        catchError(this.handleError('listPackageFiles', error => new ResultSet<PackageFile>(error)))
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
        catchError(this.handleError('generatePackageList', error => new ExecutionResult<RobotType>(undefined, error)))
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
        catchError(this.handleError('uploadPackageFile', error => new ExecutionResult<RobotType>(undefined, error)))
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
        catchError(this.handleError('uploadBlockSet', error => new ExecutionResult<RobotType>(undefined, error)))
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
        catchError(this.handleError('parseImportFile', error => new ExecutionResult<RobotType>(new RobotType(), error))),
      );
  }

  updateAllowedValues(robotType: RobotType, values: NamedValue[], message?: string): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/values`;
    this.log(`Updating values for robot type ${robotType.id}`);
    const data = {
      items: values,
    };
    return this.http.post<ExecutionResult<any>>(url, data)
      .pipe(
        tap(result => {
          this.log('Updated values');
          result.message = message;
          result.output = robotType;
        }),
        catchError(this.handleError('updateAllowedValues', error => new ExecutionResult<RobotType>(undefined, error, message)))
      );
  }

  addLoggingTemplate(robotType: RobotType, template: LoggingTemplate, message?: string): Observable<ExecutionResult<any>> {
    const url = `${environment.apiURL}v1/robots/types/${robotType.id}/loggingTemplates`;
    this.log(`Adding logging template for robot type ${robotType.id}`);
    return this.http.post<ExecutionResult<any>>(url, template)
      .pipe(
        tap(result => {
          this.log(`Added template to ${robotType.id}`);
          result.message = message;
          result.output = robotType;
        }),
        catchError(this.handleError('addLoggingTemplate', error => new ExecutionResult<RobotType>(undefined, error, message)))
      );
  }
}
