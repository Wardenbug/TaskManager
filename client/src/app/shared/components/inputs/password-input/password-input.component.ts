import { Component, inject, Input, signal } from '@angular/core';
import { ControlContainer, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-password-input',
  imports: [
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './password-input.component.html',
  styleUrl: './password-input.component.scss',
  viewProviders: [
    {
      provide: ControlContainer,
      useFactory: () => inject(ControlContainer, { skipSelf: true }),
    }
  ]
})
export class PasswordInputComponent {

  @Input({ required: true }) controlKey = '';
  @Input() label = "Enter your password";
  parentContainer = inject(ControlContainer);

  errorMessage = signal("");

  get parentFormGroup() {
    return this.parentContainer.control as FormGroup;
  }


  hide = signal(true);

  clickEvent(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }

  updateErrorMessage() {
    console.log(this.parentFormGroup.get(this.controlKey));
    if (this.parentFormGroup.get(this.controlKey)?.hasError('required')) {
      this.errorMessage.set('You must enter a value');
    } else if (this.parentFormGroup.get(this.controlKey)?.invalid) {
      var errorKeys = Object.keys(this.parentFormGroup.get(this.controlKey)?.errors as {});

      this.errorMessage.set('Invalid password');

      throw new Error(`Unhandled validation errors for ${this.controlKey}: ${errorKeys.join(', ')}`);
    }
    else {
      this.errorMessage.set('');
    }
  }
}
