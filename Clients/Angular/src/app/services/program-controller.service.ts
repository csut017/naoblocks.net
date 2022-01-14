import { EventEmitter, Injectable } from '@angular/core';
import { RunSettings } from '../data/run-settings';

@Injectable({
  providedIn: 'root'
})
export class ProgramControllerService {

  private _isPlaying: boolean = false;
  onPlay: EventEmitter<RunSettings> = new EventEmitter();
  onStop: EventEmitter<any> = new EventEmitter();

  constructor() { }

  play(settings: RunSettings): void {
    this.onPlay.emit(settings);
  }
  
  stop(): void {
    this.onStop.emit();
  }
  
  public get isPlaying(): boolean {
    return this._isPlaying;
  }
  
  public set isPlaying(value: boolean) {
    this._isPlaying = value;
  }

}
