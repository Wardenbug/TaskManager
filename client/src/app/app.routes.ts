import { Routes } from '@angular/router';

export const routes: Routes = [
    {path: '', loadComponent: () => import('./features/tasks/pages/task-list-page/task-list-page.component').then(m => m.TaskListPageComponent)},
    {path: 'auth', loadChildren: () => import('./features/auth/auth.routes').then(m => m.routes)},
];
