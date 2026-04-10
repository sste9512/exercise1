import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AuthErrorResponse } from '../../../core/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  username = '';
  password = '';
  email = '';
  isSignUpMode = signal(false);
  isLoading = signal(false);
  errorMessage = signal('');
  errorDetails = signal<string[]>([]);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  toggleMode(): void {
    this.isSignUpMode.update(mode => !mode);
    this.errorMessage.set('');
    this.errorDetails.set([]);
  }

  onSubmit(): void {
    if (!this.username || !this.password) {
      this.errorMessage.set('Please fill in all required fields');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.errorDetails.set([]);

    if (this.isSignUpMode()) {
      this.authService.signUp({
        username: this.username,
        password: this.password,
        email: this.email || undefined
      }).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.handleAuthError(error.error, 'Sign up failed. Please try again.');
        }
      });
    } else {
      this.authService.login({
        username: this.username,
        password: this.password
      }).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.handleAuthError(error.error, 'Invalid username or password');
        }
      });
    }
  }

  private handleAuthError(errorResponse: AuthErrorResponse | undefined, fallbackMessage: string): void {
    if (!errorResponse) {
      this.errorMessage.set(fallbackMessage);
      return;
    }

    this.errorMessage.set(errorResponse.message || fallbackMessage);

    if (errorResponse.errors && errorResponse.errors.length > 0) {
      this.errorDetails.set(errorResponse.errors.map(e => e.description));
    }
  }
}
