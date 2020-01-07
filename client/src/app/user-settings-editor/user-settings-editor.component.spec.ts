import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserSettingsEditorComponent } from './user-settings-editor.component';

describe('UserSettingsEditorComponent', () => {
  let component: UserSettingsEditorComponent;
  let fixture: ComponentFixture<UserSettingsEditorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserSettingsEditorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserSettingsEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
