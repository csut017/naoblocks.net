import { CompilationError } from './compilation-error';

describe('CompilationError', () => {
  it('should create an instance', () => {
    expect(new CompilationError()).toBeTruthy();
  });
});
