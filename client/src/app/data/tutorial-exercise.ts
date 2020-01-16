import { TutorialExerciseLine } from './tutorial-exercise-line';

export class TutorialExercise {
    name: string;
    title: string;
    order: number;
    lines: TutorialExerciseLine[];

    isCurrent: boolean;
    isLast: boolean;
}
