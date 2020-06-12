import { Component, OnInit } from '@angular/core';
import { User } from '../data/user';
import { ResultSet } from '../data/result-set';
import { FileDownloaderService } from '../services/file-downloader.service';
import { forkJoin } from 'rxjs';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit {

  isLoading: boolean = false;
  isInList: boolean = true;
  isInEditor: boolean = false;
  isNew: boolean = false;
  selected: User[] = [];
  users: ResultSet<User> = new ResultSet<User>();
  currentUser: User;
  message: string;
  errorMessage: string;

  constructor(private userService: UserService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = true;
    this.currentUser = new User(true);
  }

  doDelete() {
    forkJoin(this.selected.map(s => this.userService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        this.message = `Deleted ${successful.length} users`;
        if (failed.length !== 0) {
          this.errorMessage = `Failed to delete ${failed.length} users`;
        } else {
          this.errorMessage = undefined;
        }

        this.users.items = this.users
            .items
            .filter(el => !successful.includes(el));
        this.users.count -= successful.length;
      });
  }

  doEdit() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = false;
    this.currentUser = this.selected[0];
    if (!this.currentUser.isFullyLoaded) {
      this.userService.get(this.currentUser.id)
        .subscribe(s => this.currentUser = s.output);
    }
  }

  doExportList(): void {
    this.downloaderService.download('v1/users/export/list', 'users-list.xlsx');
  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    this.isInList = true;
    if (saved) {
      if (this.isNew) {
        this.users.items.push(this.currentUser);
        this.message = `Added user '${this.currentUser.name}'`;
        this.currentUser.id = this.currentUser.name;
      } else {
        this.message = `Updated user '${this.currentUser.name}'`;
      }
    }
  }

  private loadList(): void {
    this.isLoading = true;
    this.userService.list()
      .subscribe(data => {
        this.users = data;
        this.isLoading = false;
      });
  }

}
