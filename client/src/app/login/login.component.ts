import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../services/authentication.service';
import { ActivatedRoute, Router } from '@angular/router';
import { SystemService } from '../services/system.service';
import { SystemVersion } from '../data/system-version';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  password: string;
  username: string;
  userrole: string = 'Student';
  version: SystemVersion;
  loggingIn: boolean = false;

  loginError: string;

  constructor(private authenticationService: AuthenticationService,
    private systemService: SystemService,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit() {
    this.systemService.getVersion()
      .subscribe(v => this.version = v);
    this.route.queryParams
      .pipe(
        filter(params => params.key)
      )
      .subscribe(params => {
        console.log(`Validating login key: ${params.key}`);
        this.loggingIn = true;
        this.authenticationService.loginViaToken(params.key)
          .subscribe(data => {
            this.loggingIn = false;
            if (data.successful) {
              this.loginError = null;
              this.router.navigateByUrl(this.userrole.toLowerCase());
            } else {
              this.loginError = data.msg;
            }
          });
      });
  }

  onSubmit() {
    if (this.username && this.password) {
      console.log('Logging in');
      this.loggingIn = true;
      this.authenticationService.login(this.username, this.password, this.userrole)
        .subscribe(data => {
          this.loggingIn = false;
          if (data.successful) {
            this.loginError = null;
            this.router.navigateByUrl(this.userrole.toLowerCase());
          } else {
            this.loginError = data.msg;
          }
        });
    }
  }

}
