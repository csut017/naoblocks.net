import { Component, OnInit } from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { SystemVersion } from 'src/app/data/system-version';
import { AuthenticationService, LoginResult } from 'src/app/services/authentication.service';
import { SystemService } from 'src/app/services/system.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  form: UntypedFormGroup;
  loginInvalid = false;
  version?: SystemVersion;
  loggingIn: boolean = false;
  returnTo?: string;
  reason?: string;

  constructor(private authenticationService: AuthenticationService,
    private systemService: SystemService,
    private route: ActivatedRoute,
    private router: Router,
    builder: UntypedFormBuilder) {
    this.form = builder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
    this.route.queryParams.subscribe(params => {
      this.returnTo = params['return'];
      this.reason = params['reason'];
      switch (this.reason) {
        case 'expired':
          this.reason = 'Your session has expired, please login again';
          break;
      }
    });
  }

  ngOnInit(): void {
    this.systemService.getVersion()
      .subscribe(v => this.version = v);
    this.route.queryParams
      .pipe(
        filter(params => params['key'])
      )
      .subscribe(params => {
        console.log(`Validating login key: ${params['key']}`);
        this.loggingIn = true;
        this.authenticationService.loginViaToken(params['key'])
          .subscribe(data => this.handleLogin(data));
      });
  }

  onSubmit() {
    const username = this.form.get('username')?.value;
    const password = this.form.get('password')?.value;
    if (username && password) {
      console.log('Logging in');
      this.loggingIn = true;
      this.authenticationService.login(username, password, 'student')
        .subscribe(data => this.handleLogin(data));
    }
  }

  attemptQRLogin(key: string) {
    console.log(`Validating login key: ${key}`);
    this.loggingIn = true;
    this.authenticationService.loginViaToken(key)
      .subscribe(data => this.handleLogin(data));
}

  private handleLogin(data: LoginResult) {
    this.loggingIn = false;
    if (data.successful) {
      this.loginInvalid = false;
      let view = this.returnTo;
      if (!view) {
        switch (data.output?.defaultView) {
          case 1:
            view = 'student/tangibles';
            break;

          case 2:
            view = data.output.role?.toLowerCase();
            break;

          default:
            view = 'student/blockly';
            break;
        }
      }

      this.router.navigateByUrl(view.toLowerCase());
    } else {
      console.log(data.msg);
      this.loginInvalid = true;
    }
  }
}
