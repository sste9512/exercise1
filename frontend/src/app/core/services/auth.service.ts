import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, timeout } from 'rxjs';
import { LoginRequest, LoginResponse, SignUpRequest, SignUpResponse } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = '/api/auth';
  private readonly CREDENTIALS_KEY = 'auth_credentials';
  
  isAuthenticated = signal<boolean>(false);
  currentUser = signal<LoginResponse | null>(null);

  constructor(private http: HttpClient) {
    this.checkStoredCredentials();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/login`, credentials).pipe(
      timeout(30000),
      tap(response => {
        this.storeCredentials(credentials.username, credentials.password);
        this.isAuthenticated.set(true);
        this.currentUser.set(response);
      })
    );
  }

  signUp(request: SignUpRequest): Observable<SignUpResponse> {
    return this.http.post<SignUpResponse>(`${this.API_URL}/signup`, request).pipe(
      timeout(30000),
      tap(response => {
        this.storeCredentials(request.username, request.password);
        this.isAuthenticated.set(true);
        this.currentUser.set({
          username: response.username,
          roles: response.roles
        });
      })
    );
  }

  logout(): Observable<boolean> {
    return this.http.post<boolean>(`${this.API_URL}/logout`, {}).pipe(
      tap(() => {
        this.clearCredentials();
        this.isAuthenticated.set(false);
        this.currentUser.set(null);
      })
    );
  }

  getStoredCredentials(): { username: string; password: string } | null {
    const stored = localStorage.getItem(this.CREDENTIALS_KEY);
    if (stored) {
      try {
        return JSON.parse(atob(stored));
      } catch {
        return null;
      }
    }
    return null;
  }

  private storeCredentials(username: string, password: string): void {
    const credentials = { username, password };
    localStorage.setItem(this.CREDENTIALS_KEY, btoa(JSON.stringify(credentials)));
  }

  private clearCredentials(): void {
    localStorage.removeItem(this.CREDENTIALS_KEY);
  }

  private checkStoredCredentials(): void {
    const credentials = this.getStoredCredentials();
    if (credentials) {
      this.isAuthenticated.set(true);
    }
  }
}
