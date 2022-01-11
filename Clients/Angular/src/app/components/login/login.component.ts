import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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
  form: FormGroup;
  loginInvalid = false;
  version?: SystemVersion;
  loggingIn: boolean = false;
  returnTo?: string;
  reason?: string;

  constructor(private authenticationService: AuthenticationService,
    private systemService: SystemService,
    private route: ActivatedRoute,
    private router: Router,
    builder: FormBuilder) {
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

  private handleLogin(data: LoginResult) {
    this.loggingIn = false;
    if (data.successful) {
      this.loginInvalid = false;
      const view = this.returnTo || data.output?.view || 'student';
      this.router.navigateByUrl(view.toLowerCase());
    } else {
      console.log(data.msg);
      this.loginInvalid = true;
    }
  }
}
