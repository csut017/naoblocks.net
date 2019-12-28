import { TestBed } from '@angular/core/testing';

import { CommunicationsService } from './communications.service';

describe('CommunicationsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: CommunicationsService = TestBed.get(CommunicationsService);
    expect(service).toBeTruthy();
  });
});
