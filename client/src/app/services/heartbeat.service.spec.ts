import { TestBed } from '@angular/core/testing';

import { HeartbeatService } from './heartbeat.service';

describe('HeartbeatService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: HeartbeatService = TestBed.get(HeartbeatService);
    expect(service).toBeTruthy();
  });
});
