import { TestBed } from '@angular/core/testing';

import { AuthenticationGuardService } from './authentication-guard.service';

describe('AuthenticationGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AuthenticationGuardService = TestBed.get(AuthenticationGuardService);
    expect(service).toBeTruthy();
  });
});
