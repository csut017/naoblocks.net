import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SystemVersion } from 'src/app/data/system-version';
import { SystemService } from 'src/app/services/system.service';

@Component({
  selector: 'app-system-initialisation',
  templateUrl: './system-initialisation.component.html',
  styleUrls: ['./system-initialisation.component.scss']
})
export class SystemInitialisationComponent implements OnInit {

  initialisationFailed = false;
  version?: SystemVersion;
  initialising: boolean = false;

  formGroup = this.formBuilder.group({
    password: ['', Validators.required],
    useDefaultUi: false,
    addNaoRobot: false,
  });

  constructor(private systemService: SystemService,
    private router: Router,
    private formBuilder: FormBuilder) {
  }

  ngOnInit(): void {
    this.systemService.getVersion()
      .subscribe(v => this.version = v);
  }
  
  onSubmit(formGroup: FormGroup) {
    const password = formGroup.get('password')?.value;
    const useDefaultUi = formGroup.get('useDefaultUi')?.value;
    const addNaoRobot = formGroup.get('addNaoRobot')?.value;
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
