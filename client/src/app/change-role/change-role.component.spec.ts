import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeRoleComponent } from './change-role.component';

describe('ChangeRoleComponent', () => {
  let component: ChangeRoleComponent;
  let fixture: ComponentFixture<ChangeRoleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChangeRoleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChangeRoleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
