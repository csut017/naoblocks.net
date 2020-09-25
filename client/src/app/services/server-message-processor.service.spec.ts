import { TestBed } from '@angular/core/testing';

import { ServerMessageProcessorService } from './server-message-processor.service';

describe('ServerMessageProcessorService', () => {
  let service: ServerMessageProcessorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerMessageProcessorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
