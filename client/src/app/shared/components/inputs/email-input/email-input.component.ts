import { Component, inject, Input, signal } from '@angular/core';
import { ControlContainer, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormFieldBase } from '../../../abstractions/form-field-base';

@Component({
  selector: 'app-email-input',
  imports: [
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
  templateUrl: './email-input.component.html',
  styleUrl: './email-input.component.scss',
  viewProviders: [
    {
      provide: ControlContainer,
      useFactory: () => inject(ControlContainer, { skipSelf: true }),
    }
  ]
})
export class EmailInputComponent extends FormFieldBase {
  constructor() {
    super();
    this.label = this.label || 'Enter your email';
  }

  protected override handleSpecificErrors(): void {
    if (this.control?.hasError('email')) {
      this.errorMessage.set('Not a valid email');
    } else {
      super.handleSpecificErrors();
    }
  }
}
