import { EventEmitter, Injectable } from '@angular/core';
import { ControllerAction } from '../data/controller-action';
import { ControllerEvent } from '../data/controller-event';
import { RunSettings } from '../data/run-settings';

@Injectable({
  providedIn: 'root'
})
export class ProgramControllerService {

  private _isPlaying: boolean = false;
  onAction: EventEmitter<ControllerEvent> = new EventEmitter();
  workspaceXml?: string;

  constructor() { }

  clear(): void {
    let event = new ControllerEvent(ControllerAction.clear);
    this.onAction.emit(event);
  }

  play(settings: RunSettings): void {
    let event = new ControllerEvent(ControllerAction.play);
    event.data = settings;
    this.onAction.emit(event);
  }

  stop(): void {
    let event = new ControllerEvent(ControllerAction.stop);
    this.onAction.emit(event);
  }

  public get isPlaying(): boolean {
    return this._isPlaying;
  }

  public set isPlaying(value: boolean) {
    this._isPlaying = value;
  }

}
