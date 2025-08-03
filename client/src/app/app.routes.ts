import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./features/tasks/pages/task-list-page/task-list-page.component')
                .then(m => m.TaskListPageComponent),
        canActivate: [authGuard]
    },
    {
        path: 'create',
        loadComponent: () =>
            import('./features/tasks/pages/create-task-page/create-task-page.component')
                .then(m => m.CreateTaskPageComponent),
        canActivate: [authGuard]
    },

    { path: 'auth', loadChildren: () => import('./features/auth/auth.routes').then(m => m.routes) },
];
