# University Management System - Frontend Dashboard

## 📋 Overview
A modern Angular dashboard for the University Management System with role-based access, real-time notifications, and comprehensive analytics.

## 🏗️ Architecture
- **Angular 17+** with standalone components
- **Modular Structure** with feature modules
- **Role-based Routing** with guards
- **JWT Authentication** with refresh tokens
- **Real-time Notifications** with SignalR
- **Responsive Design** with Tailwind CSS

## 🚀 Features

### 🔐 Authentication System
- Login/Logout with JWT tokens
- Automatic token refresh
- Role-based access control
- Session management

### 📊 Dashboard Analytics
- Real-time statistics
- Interactive charts
- Performance metrics
- System health monitoring

### 👥 User Management
- Profile management
- File uploads
- Activity tracking
- Settings management

### 🔔 Notifications
- Real-time alerts
- Notification center
- System alerts
- User notifications

### 🎓 Academic Management
- Student dashboard
- Teacher dashboard
- Course management
- Grade tracking

## 📁 Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/           # Core services
│   │   │   ├── interceptors/       # HTTP interceptors
│   │   │   ├── guards/            # Route guards
│   │   │   └── models/            # Data models
│   │   ├── modules/
│   │   │   ├── auth/              # Authentication module
│   │   │   ├── admin/             # Admin dashboard
│   │   │   ├── student/           # Student dashboard
│   │   │   ├── teacher/           # Teacher dashboard
│   │   │   └── shared/            # Shared components
│   │   ├── shared/                # Shared utilities
│   │   └── app-routing.module.ts   # App routing
│   ├── assets/                    # Static assets
│   └── environments/              # Environment configs
├── angular.json                   # Angular configuration
├── package.json                   # Dependencies
└── README.md                      # This file
```

## 🛠️ Setup Instructions

### Prerequisites
- Node.js 18+ 
- Angular CLI 17+
- Git

### Installation
```bash
# Install Angular CLI
npm install -g @angular/cli

# Create new project
ng new university-dashboard --routing --style=scss

# Navigate to project
cd university-dashboard

# Install additional dependencies
npm install @angular/material @angular/cdk
npm install tailwindcss
npm install jwt-decode
npm install chart.js ng2-charts
npm install @ngx-translate/core

# Start development server
ng serve -o
```

## 🔧 Configuration

### Environment Setup
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  wsUrl: 'http://localhost:5000/hubs'
};
```

### Tailwind CSS Setup
```bash
# Initialize Tailwind
npx tailwindcss init

# Configure tailwind.config.js
# Update styles.scss
```

## 🌐 Application Structure

### Core Services
- **AuthService** - Authentication and token management
- **ApiService** - HTTP client with interceptors
- **NotificationService** - Real-time notifications
- **UserService** - User profile management

### Interceptors
- **JwtInterceptor** - Adds JWT token to requests
- **RefreshTokenInterceptor** - Handles token refresh
- **ErrorInterceptor** - Global error handling

### Guards
- **AuthGuard** - Authentication guard
- **AdminGuard** - Admin role guard
- **StudentGuard** - Student role guard
- **TeacherGuard** - Teacher role guard

### Feature Modules
- **AuthModule** - Login, register, password reset
- **AdminModule** - Admin dashboard and management
- **StudentModule** - Student dashboard and features
- **TeacherModule** - Teacher dashboard and features
- **SharedModule** - Common components and pipes

## 🚀 Getting Started

### 1. Create Project Structure
```bash
# Create core directories
mkdir -p src/app/core/{services,interceptors,guards,models}
mkdir -p src/app/modules/{auth,admin,student,teacher,shared}
```

### 2. Generate Modules and Components
```bash
# Generate auth module
ng g module modules/auth --route auth --module app.module
ng g component modules/auth/login
ng g component modules/auth/register

# Generate admin module
ng g module modules/admin --route admin --module app.module
ng g component modules/admin/dashboard
ng g component modules/admin/users

# Generate student module
ng g module modules/student --route student --module app.module
ng g component modules/student/dashboard
ng g component modules/student/courses

# Generate teacher module
ng g module modules/teacher --route teacher --module app.module
ng g component modules/teacher/dashboard
ng g component modules/teacher/courses
```

### 3. Create Core Services
```bash
# Generate services
ng g service core/services/auth
ng g service core/services/api
ng g service core/services/notification
ng g service core/services/user

# Generate interceptors
ng g interceptor core/interceptors/jwt
ng g interceptor core/interceptors/refresh-token
ng g interceptor core/interceptors/error

# Generate guards
ng g guard core/guards/auth
ng g guard core/guards/admin
ng g guard core/guards/student
ng g guard core/guards/teacher
```

## 🔐 Authentication Flow

### Login Process
1. User enters credentials
2. AuthService calls `/api/auth/login`
3. Backend returns JWT + Refresh Token
4. Tokens stored in localStorage
5. User redirected based on role

### Token Management
- **Access Token**: 60 minutes, sent with each request
- **Refresh Token**: 7 days, used to get new access token
- **Auto Refresh**: Interceptor handles 401 responses

### Role-based Routing
```typescript
// Admin routes
{
  path: 'admin',
  canActivate: [AuthGuard, AdminGuard],
  loadChildren: () => import('./modules/admin/admin.module').then(m => m.AdminModule)
}

// Student routes
{
  path: 'student',
  canActivate: [AuthGuard, StudentGuard],
  loadChildren: () => import('./modules/student/student.module').then(m => m.StudentModule)
}

// Teacher routes
{
  path: 'teacher',
  canActivate: [AuthGuard, TeacherGuard],
  loadChildren: () => import('./modules/teacher/teacher.module').then(m => m.TeacherModule)
}
```

## 📊 Dashboard Features

### Admin Dashboard
- System statistics
- User management
- Course management
- Grade analytics
- System health

### Student Dashboard
- Profile information
- Course enrollment
- Grade tracking
- Notifications
- Schedule

### Teacher Dashboard
- Course management
- Student management
- Grade entry
- Analytics
- Notifications

## 🎨 UI/UX Features

### Responsive Design
- Mobile-first approach
- Tailwind CSS for styling
- Material Design components
- Dark/light theme support

### Interactive Elements
- Charts and graphs
- Data tables with sorting/filtering
- Modal dialogs
- Toast notifications
- Loading states

### Accessibility
- ARIA labels
- Keyboard navigation
- Screen reader support
- High contrast mode

## 🔧 Development Tools

### Code Quality
- ESLint configuration
- Prettier formatting
- Husky pre-commit hooks
- Unit tests with Jest
- E2E tests with Cypress

### Performance
- Lazy loading modules
- OnPush change detection
- TrackBy functions
- Bundle optimization
- Image optimization

## 🚀 Deployment

### Build Process
```bash
# Development build
ng build

# Production build
ng build --configuration production

# Serve locally
ng serve --configuration production
```

### Docker Deployment
```dockerfile
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN ng build --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/university-dashboard /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## 📚 API Integration

### Backend Endpoints
- Authentication: `/api/auth/*`
- Profile: `/api/userprofile/*`
- Dashboard: `/api/dashboard/*`
- Notifications: `/api/notification/*`
- Files: `/api/file/*`

### Data Models
```typescript
// User model
interface User {
  id: number;
  email: string;
  fullName: string;
  role: string;
  profilePictureUrl?: string;
}

// Dashboard stats
interface DashboardStats {
  totalStudents: number;
  totalTeachers: number;
  totalCourses: number;
  averageGrade: number;
}

// Notification
interface Notification {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}
```

## 🔄 Real-time Features

### SignalR Integration
```typescript
// Hub connection for real-time updates
private hubConnection: signalr.HubConnection;

connect(): void {
  this.hubConnection = new signalr.HubConnectionBuilder()
    .withUrl(environment.wsUrl + '/notificationHub')
    .build();
    
  this.hubConnection.on('ReceiveNotification', (notification) => {
    this.notificationService.addNotification(notification);
  });
}
```

## 📱 Mobile Support

### Progressive Web App
- Service worker for offline support
- App manifest for installation
- Responsive design for all devices
- Touch-friendly interface

## 🧪 Testing

### Unit Tests
```bash
# Run all tests
ng test

# Run with coverage
ng test --code-coverage

# Run specific test
ng test --testNamePattern="AuthService"
```

### E2E Tests
```bash
# Run E2E tests
ng e2e

# Run specific test
ng e2e --spec=login.e2e.ts
```

## 📞 Support

For development support:
- Check the documentation
- Review the API endpoints
- Test with backend running
- Check browser console for errors

## 🤝 Contributing

1. Follow the coding standards
- Write tests for new features
- Update documentation
- Submit pull requests

---

**Built with ❤️ using Angular 17+**
