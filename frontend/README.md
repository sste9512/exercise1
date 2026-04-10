# Stargate API Frontend

Modern Angular 17 frontend application for the Stargate API with authentication and dashboard.

## Features

- ✅ **Modern Angular 17** with standalone components
- ✅ **Beautiful Login Screen** with sign-in and sign-up functionality
- ✅ **Basic Authentication** with HTTP interceptor
- ✅ **Route Guards** for protected routes
- ✅ **Responsive Design** with modern UI/UX
- ✅ **Signal-based State Management**
- ✅ **TypeScript** for type safety

## Prerequisites

- Node.js 18+ and npm
- Angular CLI 17+

## Installation

```bash
# Install dependencies
npm install

# Install Angular CLI globally (if not already installed)
npm install -g @angular/cli
```

## Development Server

```bash
# Start the development server
npm start

# Or use Angular CLI directly
ng serve
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## API Configuration

The frontend is configured to proxy API requests to the backend:

- **Frontend**: `http://localhost:4200`
- **Backend API**: `https://localhost:7076` (proxied through `/api`)

The proxy configuration is in `proxy.conf.json`. Update the target URL if your API runs on a different port.

## Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── guards/          # Route guards (auth.guard.ts)
│   │   ├── interceptors/    # HTTP interceptors (auth.interceptor.ts)
│   │   ├── models/          # TypeScript interfaces
│   │   └── services/        # Services (auth.service.ts)
│   ├── features/
│   │   ├── auth/
│   │   │   └── login/       # Login component
│   │   └── dashboard/       # Dashboard component
│   ├── app.component.ts     # Root component
│   └── app.routes.ts        # Route configuration
├── index.html
├── main.ts
└── styles.scss              # Global styles
```

## Authentication Flow

1. **Login/Sign Up**: Users can sign in or create a new account
2. **Credentials Storage**: Username and password are stored in localStorage (Base64 encoded)
3. **HTTP Interceptor**: Automatically adds Basic Authentication header to API requests
4. **Route Guard**: Protects dashboard route, redirects to login if not authenticated
5. **Logout**: Clears credentials and redirects to login

## Available Routes

- `/login` - Login and sign-up page
- `/dashboard` - Protected dashboard (requires authentication)
- `/` - Redirects to login

## Building for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## API Endpoints Used

- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration
- `POST /api/auth/logout` - User logout

## Styling

The application uses:
- **SCSS** for styling
- **Modern gradient backgrounds**
- **Smooth animations and transitions**
- **Responsive design** for mobile and desktop
- **Custom form controls** with focus states

## Security Notes

⚠️ **Important**: This implementation stores credentials in localStorage for demonstration purposes. In a production environment, consider:

- Using JWT tokens instead of storing passwords
- Implementing refresh token mechanism
- Using secure, httpOnly cookies
- Adding CSRF protection
- Implementing proper session management
