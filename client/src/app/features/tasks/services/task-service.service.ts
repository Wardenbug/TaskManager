import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Task } from '../types/task';
import { CreateTaskDto } from '../models/create-task-dto';
import { UpdateTaskDto } from '../models/update-task-dto';
import { PaginationParams } from '../types/pagination-params';
import { toHttpParams } from '../../../shared/utils/to-http-params';
import { PaginatedResult } from '../../../shared/abstractions/paginated-result';

@Injectable({
  providedIn: 'root'
})
export class TaskServiceService {

  private readonly http: HttpClient = inject(HttpClient);

  public createTask(task: CreateTaskDto) {
    return this.http.post<Task>('api/tasks', task);
  }

  public updateTask(id: string, task: UpdateTaskDto) {
    return this.http.put<Task>(`api/tasks/${id}`, task);
  }

  public deleteTask(id: string) {
    return this.http.delete<void>(`api/tasks/${id}`);
  }

  public getTasks(paginationParams?: PaginationParams) {
    return this.http.get<PaginatedResult<Task>>('api/tasks', { params: toHttpParams(paginationParams) });
  }

  public getTaskById(id: string) {
    return this.http.get<Task>(`api/tasks/${id}`);
  }

  constructor() { }
}
