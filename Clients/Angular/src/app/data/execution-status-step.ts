export class ExecutionStatusStep {
    image: string = 'circle';
    title: string;
    description: string;
    isCurrent: boolean = false;
  
    constructor(title: string, description: string) {
      this.title = title;
      this.description = description;
    }
  }
