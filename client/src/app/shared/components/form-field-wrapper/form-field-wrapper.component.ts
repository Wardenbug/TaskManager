import { Component, inject, Input, signal } from '@angular/core';
import { ControlContainer, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatError, MatLabel } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-field-wrapper',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatError,
    MatLabel,
  ],
  templateUrl: './form-field-wrapper.component.html',
  styleUrl: './form-field-wrapper.component.scss',
})
export class FormFieldWrapperComponent {
  @Input({ required: true }) controlKey = '';
  @Input() label = '';
  @Input() errorMessage = signal('');

  parentContainer = inject(ControlContainer);

  get parentFormGroup(): FormGroup {
    return this.parentContainer.control as FormGroup;
  }
}