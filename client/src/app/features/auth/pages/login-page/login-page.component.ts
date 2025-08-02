import { Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';
import { EmailInputComponent } from '../../../../shared/components/inputs/email-input/email-input.component';
import { PasswordInputComponent } from "../../../../shared/components/inputs/password-input/password-input.component";

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    RouterLink,
    EmailInputComponent,
    PasswordInputComponent
],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {

  private readonly authService: AuthService = inject(AuthService);
  private readonly router: Router = inject(Router);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)])
  });

  onSubmit() {
    console.log('Form Submitted', this.loginForm.value);

    const { email, password } = this.loginForm.value;

    if (typeof email !== 'string' || email.trim() === '') {
      // this.errorMessage.set('Email is required.');
      return;
    }

    if (typeof password !== 'string' || password.trim() === '') {
      // this.errorMessage.set('Password is required.');
      return;
    }

    const userLogin = {
      email: email as string,
      password: password as string
    };

    this.authService.login(userLogin).subscribe({
      next: (user) => {
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Login failed', error);
        // this.errorMessage.set('Login failed. Please check your credentials.');
      },
      complete: () => console.info('Login request completed')
    })
  }
}
