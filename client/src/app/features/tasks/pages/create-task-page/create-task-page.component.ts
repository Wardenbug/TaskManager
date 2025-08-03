import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { TextInputComponent } from '../../../../shared/components/inputs/text-input/text-input.component';
import { TaskServiceService } from '../../services/task-service.service';
import { CreateTaskDto } from '../../models/create-task-dto';

@Component({
  selector: 'app-create-task-page',
  imports: [
    MatCardModule,
    ReactiveFormsModule,
    MatButtonModule,
    TextInputComponent
  ],
  templateUrl: './create-task-page.component.html',
  styleUrl: './create-task-page.component.scss'
})
export class CreateTaskPageComponent {

  private readonly taskService = inject(TaskServiceService);
  private readonly router = inject(Router);

  createTaskForm = new FormGroup({
    title: new FormControl('', [Validators.required]),
    description: new FormControl('')
  });

  onSubmit() {
    if (!this.createTaskForm.get('title')) return;

    const createTaskDto: CreateTaskDto = {
      title: this.createTaskForm.get('title')?.value as string,
      description: this.createTaskForm.get('description')?.value || ''
    }

    this.taskService.createTask(createTaskDto).subscribe({
      next: (task) => {
        console.log('Task created successfully:', task);
        this.router.navigate(['/']);
      }
    });
  }
}
