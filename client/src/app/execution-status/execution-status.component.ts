import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ExecutionStatusStep } from '../data/execution-status-step';

@Component({
  selector: 'app-execution-status',
  templateUrl: './execution-status.component.html',
  styleUrls: ['./execution-status.component.scss']
})
export class ExecutionStatusComponent implements OnInit {

  @Input() isOpen: boolean = false;
  @Output() onClose = new EventEmitter<boolean>();
  @Input() steps: ExecutionStatusStep[] = [];
  @Input() statusMessage: string;

  constructor() { }

  ngOnInit(): void {
  }

  doClose(): void {
    this.onClose.emit(!this.statusMessage);
  }

}
