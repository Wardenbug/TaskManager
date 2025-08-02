import { Component, inject } from '@angular/core';
import { FormFieldBase } from '../../../abstractions/form-field-base';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ControlContainer, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-text-input',
  imports: [
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.scss',
  viewProviders: [
    {
      provide: ControlContainer,
      useFactory: () => inject(ControlContainer, { skipSelf: true }),
    }
  ]
})
export class TextInputComponent extends FormFieldBase {
  constructor() {
    super();
    this.label = this.label || 'Enter text';
  }
}
