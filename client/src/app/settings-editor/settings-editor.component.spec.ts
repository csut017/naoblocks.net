import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SettingsEditorComponent } from './settings-editor.component';

describe('SettingsEditorComponent', () => {
  let component: SettingsEditorComponent;
  let fixture: ComponentFixture<SettingsEditorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsEditorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettingsEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
