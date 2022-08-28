import { Component, OnInit, Renderer2 } from '@angular/core';
import { Settings } from 'luxon';
import { environment } from 'src/environments/environment';
import { ScriptLoaderService } from './services/script-loader.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(
    private renderer: Renderer2,
    private scriptService: ScriptLoaderService,
  ) {}

  ngOnInit(): void {
    this.scriptService.loadScript(this.renderer, `${environment.apiURL}v1/ui/angular/block_definitions`);
    this.scriptService.loadScript(this.renderer, `${environment.apiURL}v1/ui/angular/language`);
    Settings.defaultLocale = 'en-NZ';
  }
}
