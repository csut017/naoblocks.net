import { TestBed } from '@angular/core/testing';

import { AstConverterService } from './ast-converter.service';

describe('AstConverterService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    expect(service).toBeTruthy();
  });
});
