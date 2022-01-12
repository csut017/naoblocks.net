import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, Renderer2 } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ScriptLoaderService {

  constructor(
    @Inject(DOCUMENT) private document: Document
  ) { }

  public loadScript(renderer: Renderer2, src: string): Observable<string> {
    console.log(`[ScriptLoaderService] Loading ${src}`);
    const script = renderer.createElement('script');
    script.type = 'text/javascript';
    script.src = src;
    renderer.appendChild(this.document.body, script);
    return new Observable<string>(subscriber => {
      script.onload = () => {
        console.log(`[ScriptLoaderService] Loaded ${src}`);
        subscriber.next(src);
        subscriber.complete();
      };
      script.onerror = () => {
        const error = `Could not load ${src}`;
        console.error(`[ScriptLoaderService] ${error}`);
        subscriber.error(error);
      };
    });
  }
}
