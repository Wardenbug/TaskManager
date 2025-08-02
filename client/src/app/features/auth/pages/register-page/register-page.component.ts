import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { passwordMatchValidator } from '../../../../shared/validators/password-match-validator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { EmailInputComponent } from '../../../../shared/components/inputs/email-input/email-input.component';
import { PasswordInputComponent } from '../../../../shared/components/inputs/password-input/password-input.component';
import { TextInputComponent } from '../../../../shared/components/inputs/text-input/text-input.component';
import { passwordMatchValidatorForControl } from '../../../../shared/validators/password-match-validator-for-control';
import { AuthService } from '../../../../core/services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { UserRegister } from '../../../../core/models/user/user-register';

@Component({
  selector: 'app-register-page',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    EmailInputComponent,
    PasswordInputComponent,
    TextInputComponent,
    RouterLink
  ],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss'
})
export class RegisterPageComponent {

  private readonly authService: AuthService = inject(AuthService);
  private readonly router: Router = inject(Router);

  passwordControl = new FormControl('', Validators.required);
  registerForm = new FormGroup({
    userName: new FormControl('', Validators.required),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: this.passwordControl,
    confirmPassword: new FormControl('', [Validators.required, passwordMatchValidatorForControl(this.passwordControl)])
  }, { validators: passwordMatchValidator });


  onSubmit() {
    console.log('Form Submitted', this.registerForm);

    const registrationData: UserRegister = {
      userName: this.registerForm.get('userName')?.value as string,
      email: this.registerForm.get('email')?.value as string,
      password: this.registerForm.get('password')?.value as string,
      confirmPassword: this.registerForm.get('confirmPassword')?.value as string
    };

    this.authService.register(registrationData).subscribe({
      next: (value) => {
        this.authService.User = value;
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Registration error:', error);
        // TODO Handle error, e.g., show a message to the user
      }
    });
  }
}
