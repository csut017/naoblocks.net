import { Component, Inject, OnInit } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';

@Component({
  selector: 'app-multiline-message',
  templateUrl: './multiline-message.component.html',
  styleUrls: ['./multiline-message.component.scss']
})
export class MultilineMessageComponent implements OnInit {

  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: string[]) { }

  ngOnInit(): void {
  }

}
