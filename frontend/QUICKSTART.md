# Quick Start Guide

## Setup and Run

### 1. Install Dependencies

```powershell
cd frontend
npm install
```

### 2. Start the Backend API

In a separate terminal, navigate to the API directory and run:

```powershell
cd ..\api
dotnet run
```

The API should start on `https://localhost:7076`

### 3. Start the Angular Frontend

```powershell
cd ..\frontend
npm start
```

The frontend will start on `http://localhost:4200`

### 4. Access the Application

Open your browser and navigate to: **http://localhost:4200**

## First Time Login

### Option 1: Sign Up (Create New Account)

1. Click "Sign Up" on the login screen
2. Enter a username and password
3. Optionally enter an email
4. Click "Sign Up"
5. You'll be automatically logged in and redirected to the dashboard

### Option 2: Use Existing Account

If you already have an account:
1. Enter your username and password
2. Click "Sign In"
3. You'll be redirected to the dashboard

## Default Test Credentials

If the database is seeded with default users, you can try:
- **Username**: `admin`
- **Password**: `admin`

## Troubleshooting

### CORS Errors
- Make sure the API is running on `https://localhost:7076`
- Check that CORS is properly configured in `Program.cs`

### API Connection Issues
- Verify the API URL in `proxy.conf.json` matches your API port
- Check that both frontend and backend are running

### Authentication Issues
- Clear browser localStorage: `localStorage.clear()` in browser console
- Try signing up with a new account

## Features to Test

✅ **Sign Up**: Create a new user account
✅ **Sign In**: Login with existing credentials  
✅ **Dashboard**: View welcome screen after login
✅ **Logout**: Sign out and return to login screen
✅ **Route Protection**: Try accessing `/dashboard` without logging in

## Development Tips

- Frontend runs on port **4200**
- Backend API runs on port **7076**
- API requests are proxied through `/api` prefix
- Hot reload is enabled for both frontend and backend
- Check browser console for any errors
- Check API logs for backend issues
