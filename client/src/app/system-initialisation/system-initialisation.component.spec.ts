import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemInitialisationComponent } from './system-initialisation.component';

describe('SystemInitialisationComponent', () => {
  let component: SystemInitialisationComponent;
  let fixture: ComponentFixture<SystemInitialisationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SystemInitialisationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SystemInitialisationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
