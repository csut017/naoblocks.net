import { Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSelectionList, MatSelectionListChange } from '@angular/material/list';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';
import { Robot } from 'src/app/data/robot';
import { RobotLog } from 'src/app/data/robot-log';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { RobotLogService } from 'src/app/services/robot-log.service';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-mobile-home',
  templateUrl: './mobile-home.component.html',
  styleUrls: ['./mobile-home.component.scss']
})
export class MobileHomeComponent extends HomeBase implements OnInit {

  @ViewChild(MatSelectionList) robotList?: MatSelectionList;

  currentView: string = 'Robots';
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  logs: RobotLog[] = [];
  robots: Robot[] = [];
  selectedRobot?: Robot;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private breakpointObserver: BreakpointObserver,
    private route: ActivatedRoute,
    private robotService: RobotService,
    private robotLogService: RobotLogService) {
    super(authenticationService, router, changeRoleService);
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Teacher);
    this.robotService.list()
      .subscribe(res => {
        this.robots = res.items;
        this.route.paramMap.subscribe(params => {
          let robotName = params.get('item');
          if (!robotName) return;

          robotName = robotName.toLowerCase();
          this.selectedRobot = this.robots.find(r => r.machineName?.toLowerCase() == robotName);
          this.loadRobot();
        });    
      });
  }

  isSelected(robot: Robot): boolean {
    return robot.id == this.selectedRobot?.id;
  }

  onRobotSelected(evt: MatSelectionListChange): void {
    this.selectedRobot = evt.options[0].value;
    this.loadRobot();
    if (this.selectedRobot) {
      this.router.navigate(['mobile', 'robots', encodeURIComponent(this.selectedRobot.machineName || 'unknown')], {});
    } else {
      this.router.navigate(['mobile', 'robots'], {});
    }
  }

  private loadRobot(): void {
    if (!this.selectedRobot) return;

    this.logs = [];
    this.robotLogService.list(this.selectedRobot.machineName || 'unknown')
      .subscribe(res => {
        this.logs = res.items;
      });
  }
}
