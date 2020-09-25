import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Block } from '../data/block';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { AuthenticationService } from '../services/authentication.service';

declare var TopCodes: any;

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent extends HomeBase implements OnInit {

  currentUser: User;
  cameraStarted: boolean = false;
  blocks: Block[] = [];
  canRun: boolean = false;

  private isInitialised: boolean = false;
  private context: CanvasRenderingContext2D;
  private blockMapping: { [index: number]: string } = {
    31: "stand_block",
    47: "stand_block",
    55: "speak_block",
    59: "dance_block",
    61: "wave_block",
    79: "raise_right_arm_block",
    93: "raise_left_arm_block",
    109: "look_right_block",
    117: "look_left_block",
    121: "walk_block",
  };


  @ViewChild('videoCanvas', { static: true }) videoCanvas: ElementRef<HTMLCanvasElement>;

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit(): void {
    console.log('[TangibleEditorComponent] retrieving context');
    this.context = this.videoCanvas.nativeElement.getContext('2d');
  }

  highlightTags(topcodes: any): void {
    this.context.fillStyle = "rgba(255, 0, 0, 0.3)";
    for (let loop = 0; loop < topcodes.length; loop++) {
      this.context.beginPath();
      this.context.arc(
        topcodes[loop].x,
        topcodes[loop].y,
        topcodes[loop].radius,
        0,
        Math.PI * 2,
        true
      );
      this.context.fill();
    }
  }

  generateBlockList(tags: any[]): Block[] {
    let output: Block[] = [];
    let last: any;
    for (let tag of tags) {
      // Filter the tags: only include those tags with a mapping and are close to the previous x position
      let image = this.blockMapping[tag.code];
      if (!image) { 
        console.groupCollapsed('[TangibleEditorComponent] Unknown tag');
        console.log(tag.code);
        console.groupEnd();
        continue; 
      }

      if (last) {
        if ((tag.x > last.x + 10) && (tag.x < last.x - 10)) {
          continue;
        }
      }

      output.push(new Block(image));
      last = tag;
    }

    return output;
  }

  startCamera(): void {
    if (!this.isInitialised) {
      console.log('[TangibleEditorComponent] Initialising callback');
      this.isInitialised = true;
      let me = this;
      TopCodes.setVideoFrameCallback("video-canvas", function (jsonString) {
        let json = JSON.parse(jsonString);
        me.highlightTags(json.topcodes);
        let blocks = me.generateBlockList(json.topcodes);
        me.canRun = !!blocks.length;
        me.blocks = blocks;
      });
    }

    if (this.cameraStarted) return;

    console.log('[TangibleEditorComponent] Starting camera');
    this.cameraStarted = true;
    TopCodes.startStopVideoScan('video-canvas');
  }

  stopCamera(): void {
    if (!this.cameraStarted) return;

    console.log('[TangibleEditorComponent] Stopping camera');
    this.cameraStarted = false;
    TopCodes.startStopVideoScan('video-canvas');
  }
}
