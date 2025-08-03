import { AfterViewInit, Component, inject, ViewChild } from '@angular/core';
import { TaskServiceService } from '../../services/task-service.service';
import { PaginatedResult } from '../../../../shared/abstractions/paginated-result';
import { Task } from '../../types/task';
import { MatCardModule } from '@angular/material/card';
import { TextInputComponent } from '../../../../shared/components/inputs/text-input/text-input.component';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatTable, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-task-list-page',
  imports: [
    MatCardModule,
    TextInputComponent,
    ReactiveFormsModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    RouterLink
  ],
  templateUrl: './task-list-page.component.html',
  styleUrl: './task-list-page.component.scss'
})
export class TaskListPageComponent implements AfterViewInit {

  displayedColumns: string[] = ['title', 'description', 'isCompleted'];
  private readonly taskService = inject(TaskServiceService);
  private readonly authService = inject(AuthService);
  public result: PaginatedResult<Task> | null = null;

  get userName() {
    return this.authService.User?.userName || 'Guest';
  }

  dataSource = new MatTableDataSource<Task>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }

  searchForm = new FormGroup({
    searchTerm: new FormControl(''),
  });

  onPageChange($event: PageEvent) {
    console.log('Page changed:', $event);
    throw new Error('Method not implemented.');
  }

  ngOnInit() {
    this.taskService.getTasks().subscribe({
      next: (tasks) => {
        this.result = tasks;
        this.dataSource.data = tasks.items;
        console.log('Tasks fetched successfully:', tasks);
      }
    });
  }

}
