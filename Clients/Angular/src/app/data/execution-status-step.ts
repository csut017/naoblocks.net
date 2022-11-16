export class ExecutionStatusStep {
    image: string = 'motion_photos_on';
    title: string;
    description: string;
    isCurrent: boolean = false;
    status: string = 'normal';
  
    constructor(title: string, description: string) {
      this.title = title;
      this.description = description;
    }
  }
