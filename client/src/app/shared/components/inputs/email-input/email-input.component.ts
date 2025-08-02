import { Component, inject, Input, signal } from '@angular/core';
import { ControlContainer, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-email-input',
  imports: [
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule
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
export class EmailInputComponent {
  @Input({ required: true }) controlKey = '';
  @Input() label = "Enter your email";
  parentContainer = inject(ControlContainer);

  errorMessage = signal("");

  get parentFormGroup() {
    return this.parentContainer.control as FormGroup;
  }

  updateErrorMessage() {
    if (this.parentFormGroup.get(this.controlKey)?.hasError('required')) {
      this.errorMessage.set('You must enter a value');
    } else if (this.parentFormGroup.get(this.controlKey)?.hasError('email')) {
      this.errorMessage.set('Not a valid email');
    } else if (this.parentFormGroup.get(this.controlKey)?.invalid) {
      var errorKeys = Object.keys(this.parentFormGroup.get(this.controlKey)?.errors as {});

      this.errorMessage.set(`Invalid ${this.controlKey}`);

      throw new Error(`Unhandled validation errors for ${this.controlKey}: ${errorKeys.join(', ')}`);
    } else {
      this.errorMessage.set('');
    }
  }
}
