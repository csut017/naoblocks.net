import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {

  opened: boolean;

  constructor() { }

  ngOnInit() {
  }

  show(): void {
    this.opened = true;
  }

}
