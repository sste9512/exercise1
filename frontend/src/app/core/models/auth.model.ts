export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  username: string;
  roles: string[];
}

export interface SignUpRequest {
  username: string;
  password: string;
  email?: string;
}

export interface SignUpResponse {
  id: number;
  username: string;
  roles: string[];
}

export interface IdentityError {
  code: string;
  description: string;
}

export interface AuthErrorResponse {
  message: string;
  errors?: IdentityError[];
}
