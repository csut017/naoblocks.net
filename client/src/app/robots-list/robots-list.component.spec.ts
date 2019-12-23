import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RobotsListComponent } from './robots-list.component';

describe('RobotsListComponent', () => {
  let component: RobotsListComponent;
  let fixture: ComponentFixture<RobotsListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RobotsListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RobotsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
