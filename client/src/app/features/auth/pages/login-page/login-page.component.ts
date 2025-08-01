import { M } from '@angular/cdk/keycodes';
import { Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {

  private readonly authService: AuthService = inject(AuthService);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required)
  });

  errorMessage = signal("");

  updateErrorMessage() {
    if (this.loginForm.get("email")?.hasError('required')) {
      this.errorMessage.set('You must enter a value');
    } else if (this.loginForm.get("email")?.hasError('email')) {
      this.errorMessage.set('Not a valid email');
    } else {
      this.errorMessage.set('');
    }
  }

  hide = signal(true);
  clickEvent(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }
  onSubmit() {
    console.log('Form Submitted', this.loginForm.value);

    const { email, password } = this.loginForm.value;

    if (typeof email !== 'string' || email.trim() === '') {
      this.errorMessage.set('Email is required.');
      return;
    }

    if (typeof password !== 'string' || password.trim() === '') {
      this.errorMessage.set('Password is required.');
      return;
    }

    const userLogin = {
      email: email as string,
      password: password as string
    };

    this.authService.login(userLogin).subscribe({
      next: (user) => {
        console.log('Login successful', user);
        // Handle successful login, e.g., redirect to dashboard
      },
      error: (error) => {
        console.error('Login failed', error);
        this.errorMessage.set('Login failed. Please check your credentials.');
      },
      complete: () => console.info('Login request completed')
    })
  }
}
