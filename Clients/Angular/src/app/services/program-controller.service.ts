import { EventEmitter, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ProgramControllerService {

  onPlay: EventEmitter<any> = new EventEmitter();

  constructor() { }

  play(): void {
    this.onPlay.emit();
  }

}
