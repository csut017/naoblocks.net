import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../services/authentication.service';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  password: string;
  username: string;
  userrole: string = 'Student';

  loginError: string;

  constructor(private authenticationService: AuthenticationService,
    private router: Router) { }

  ngOnInit() {
  }

  onSubmit() {
    if (this.username && this.password) {
      console.log('Logging in');
      this.authenticationService.login(this.username, this.password, this.userrole)
        .subscribe(data => {
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
