import { Component, OnInit, OnChanges, Input, Output, EventEmitter, SimpleChanges } from '@angular/core';
import { User } from '../data/user';
import { UserService } from '../services/user.service';
import { UserSettings } from '../data/user-settings';

@Component({
  selector: 'app-user-editor',
  templateUrl: './user-editor.component.html',
  styleUrls: ['./user-editor.component.scss']
})
export class UserEditorComponent implements OnChanges {
  @Input() user: User;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];
  showPassword: boolean = false;

  constructor(private userService: UserService) { }

  ngOnChanges(_: SimpleChanges): void {
    if (this.user) this.user.settings = this.user.settings || new UserSettings();
  }

  doSave() {
    if (!this.user.isNew && !this.showPassword) this.user.password = undefined;
    this.userService.save(this.user)
      .subscribe(result => {
        if (result.successful) {
          if (result.output) {
            this.user.whenAdded = result.output.whenAdded;
          }
          this.user.isNew = false;
          this.user.password = undefined;
          this.closed.emit(true);
        } else {
          this.errors = result.allErrors();
        }
      });
  }

  doCancel() {
    this.closed.emit(false);
  }

}
