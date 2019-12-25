import { TestBed } from '@angular/core/testing';

import { RobotService } from './robot.service';

describe('RobotService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RobotService = TestBed.get(RobotService);
    expect(service).toBeTruthy();
  });
});
