import { HubClient } from './hub-client';

describe('HubClient', () => {
  it('should create an instance', () => {
    expect(new HubClient()).toBeTruthy();
  });
});
