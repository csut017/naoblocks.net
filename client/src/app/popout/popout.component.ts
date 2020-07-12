import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { tap } from 'rxjs/operators';
import { ConnectionService } from '../services/connection.service';

@Component({
  selector: 'app-popout',
  templateUrl: './popout.component.html',
  styleUrls: ['./popout.component.scss']
})
export class PopoutComponent implements OnInit {

  isLoading: boolean = true;
  view: string;

  constructor(private route: ActivatedRoute,
    private connection: ConnectionService) { }

  ngOnInit(): void {
    // this.connection.start().subscribe(msg => this.processServerMessage(msg));

    this.route.params.subscribe(
      data => {
        console.log(data);
        this.isLoading = false;
        this.view = data.view;
      });
  }

}
