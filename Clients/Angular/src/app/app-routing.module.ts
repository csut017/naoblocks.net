import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthenticationGuardService } from './services/authentication-guard.service';
import { LandingComponent } from './components/landing/landing.component';
import { LoginComponent } from './components/login/login.component';
import { StudentHomeComponent } from './components/student-home/student-home.component';

const routes: Routes = [
  // Public (no authentication) routes
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  // Private (authenticated) routes
  { path: 'student', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
