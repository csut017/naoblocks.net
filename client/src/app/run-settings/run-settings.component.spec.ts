import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RunSettingsComponent } from './run-settings.component';

describe('RunSettingsComponent', () => {
  let component: RunSettingsComponent;
  let fixture: ComponentFixture<RunSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RunSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RunSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
