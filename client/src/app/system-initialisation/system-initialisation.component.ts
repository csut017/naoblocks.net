import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SystemService } from '../services/system.service';

@Component({
  selector: 'app-system-initialisation',
  templateUrl: './system-initialisation.component.html',
  styleUrls: ['./system-initialisation.component.scss']
})
export class SystemInitialisationComponent implements OnInit {

  password: string;
  error: string;

  constructor(private systemService: SystemService,
    private router: Router) { }

    private log(message: string) {
      const msg = `[SystemInitialisationComponent] ${message}`;
      console.log(msg);
    }
  
  ngOnInit(): void {
  }

  doSave() {
    this.log('Sending initialisation')
    this.systemService.initialise(this.password)
      .subscribe(res => {
        if (res.error) {
          this.log('Initialisation has failed');
          this.error = res.error;
        } else {
          this.log('Initialisation was successful');
          this.router.navigateByUrl('login');
        }
      })
  }

}
