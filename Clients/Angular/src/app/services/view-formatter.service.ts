import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ViewFormatterService {

  constructor() { }

  fromUrl(url: string): string {
    const view = url.split('_')
      .map(seg => seg[0].toUpperCase() + seg.substring(1))
      .join(' ');
    return view;
  }

  toUrl(view: string): string {
    const url = view.replace(' ', '_').toLowerCase();
    return url;
  }
}
