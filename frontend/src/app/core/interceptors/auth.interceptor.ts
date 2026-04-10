import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const credentials = authService.getStoredCredentials();

  if (credentials && !req.url.includes('/login') && !req.url.includes('/signup')) {
    const authHeader = 'Basic ' + btoa(`${credentials.username}:${credentials.password}`);
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: authHeader
      }
    });
    return next(clonedRequest);
  }

  return next(req);
};
