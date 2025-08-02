import { Component } from '@angular/core';
import { FormFieldBase } from '../../../abstractions/form-field-base';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { FormFieldWrapperComponent } from '../../form-field-wrapper/form-field-wrapper.component';

@Component({
  selector: 'app-text-input',
  imports: [
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    FormFieldWrapperComponent
  ],
  templateUrl: './text-input.component.html',
  styleUrl: './text-input.component.scss'
})
export class TextInputComponent extends FormFieldBase {
  constructor() {
    super();
    this.label = this.label || 'Enter text';
  }
}
