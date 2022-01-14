import { ExecutionStatusStep } from "./execution-status-step";

export class StartupStatusTracker {
    steps: ExecutionStatusStep[] = [];
    startMessage?: string;
    sendingToRobot: boolean = false;

    initialise() {
        this.steps = [
            new ExecutionStatusStep('Check Program', 'Checks that the program is valid and can run on the robot.'),
            new ExecutionStatusStep('Select Robot', 'Finds an available robot to run the program on.'),
            new ExecutionStatusStep('Send to Robot', 'Sends the program to the robot.'),
            new ExecutionStatusStep('Start Execution', 'Starts the program running on the robot.')
        ];
        this.steps[0].isCurrent = true;
        this.startMessage = undefined;
        this.sendingToRobot = true;
    }

    completeStep(step: number): number {
        if (step >= this.steps.length) return step;
        this.steps[step].isCurrent = false;
        this.steps[step].image = 'success-standard';

        if (++step >= this.steps.length) return step;
        this.steps[step].isCurrent = true;
        return step;
    }

    failStep(step: number, reason: string): void {
        this.startMessage = reason;
        if (!this.steps[step]) return;
        this.steps[step].isCurrent = false;
        this.steps[step].image = 'error-standard';
    }

    cancel(): boolean {
        if (this.sendingToRobot) {
            this.sendingToRobot = false;
            return false;
        }

        return true;
    }
}
