import { Component, OnInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { filter } from 'rxjs';
import { SystemVersion } from 'src/app/data/system-version';
import { AuthenticationService, LoginResult } from 'src/app/services/authentication.service';
import { SystemService } from 'src/app/services/system.service';

@Component({
  selector: 'app-system-initialisation',
  templateUrl: './system-initialisation.component.html',
  styleUrls: ['./system-initialisation.component.scss']
})
export class SystemInitialisationComponent implements OnInit {

  form: UntypedFormGroup;
  initialisationFailed = false;
  version?: SystemVersion;
  initialising: boolean = false;

  constructor(private systemService: SystemService,
    private router: Router,
    builder: UntypedFormBuilder) {
    this.form = builder.group({
      password: ['', Validators.required],
      useDefaultUi: [true],
      addNaoRobot: [true]
    });
  }

  ngOnInit(): void {
    this.systemService.getVersion()
      .subscribe(v => this.version = v);
  }

  onSubmit() {
    const password = this.form.get('password')?.value;
    const useDefaultUi = this.form.get('useDefaultUi')?.value || true;
    const addNaoRobot = this.form.get('addNaoRobot')?.valid || true;
    if (password) {
      console.log('Initialising system');
      this.initialising = true;
      this.systemService.initialise(password, useDefaultUi, addNaoRobot)
        .subscribe(data => this.handleLogin(data));
    }
  }

  private handleLogin(data: SystemVersion) {
    this.initialising = false;
    if (data.error) {
      console.log(data.error);
      this.initialisationFailed = true;
    } else {
      this.initialisationFailed = false;
      this.router.navigateByUrl('login');
    }
  }

}
