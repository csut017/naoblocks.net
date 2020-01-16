import { TutorialExercise } from './tutorial-exercise';

export class Tutorial {
    id: string;
    name: string;
    category: string;
    order: number;
    whenAdded: Date;
    exercises: TutorialExercise[];
    isNew: boolean;
}
