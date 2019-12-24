import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentEditorComponent } from './student-editor.component';

describe('StudentEditorComponent', () => {
  let component: StudentEditorComponent;
  let fixture: ComponentFixture<StudentEditorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StudentEditorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StudentEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
