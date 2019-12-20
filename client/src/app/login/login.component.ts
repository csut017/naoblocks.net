import { Component, OnInit } from '@angular/core';

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

  constructor() { }

  ngOnInit() {
  }

  onSubmit() {
    console.log('Logging in');
  }

}
