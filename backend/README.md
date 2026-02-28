# University Management System - Backend API

## 📋 Overview
A comprehensive University Management System API built with ASP.NET Core, featuring authentication, authorization, caching, notifications, and file management.

## 🏗️ Architecture
- **Clean Architecture** with Domain, Application, Infrastructure, and API layers
- **Entity Framework Core** for data access
- **JWT Authentication** with Refresh Tokens
- **Role-based Authorization** (Student, Teacher, Admin)
- **Memory Caching** for performance optimization
- **Comprehensive Error Handling** and Validation

## 🚀 Features

### 🔐 Authentication & Authorization
- JWT Access Tokens (60 minutes)
- Refresh Tokens (7 days)
- Role-based access control
- Token revocation and cleanup

### 📊 Dashboard & Analytics
- Real-time statistics
- Performance monitoring
- User activity tracking
- System health checks

### 🎓 Academic Management
- Student management
- Teacher management
- Course management
- Grade management
- Enrollment tracking

### 🔔 Notifications System
- Real-time notifications
- Multiple notification types
- Bulk operations
- System alerts

### 📁 File Management
- Profile picture uploads
- File validation and security
- Organized storage
- Download capabilities

### ⚡ Performance Features
- Memory caching
- Pagination
- Search and filtering
- Sorting capabilities

## 📁 Project Structure

```
backend/
├── src/
│   ├── Controllers/          # API Controllers
│   ├── Services/             # Business Logic Services
│   ├── Middleware/           # Custom Middleware
│   ├── Filters/              # Action Filters
│   ├── Helpers/              # Utility Classes
│   ├── Domain/
│   │   └── Entities/         # Domain Entities
│   ├── Application/
│   │   └── DTOs/             # Data Transfer Objects
│   └── Infrastructure/
│       ├── Data/             # Database Context
│       └── Auth/             # Authentication Services
├── Program.cs                # Application Entry Point
├── appsettings.json          # Configuration
└── UniversityManagement.API.csproj  # Project File
```

## 🔧 Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=UniversityDB;Trusted_Connection=True;"
  }
}
```

### JWT Settings
```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "UniversityAPI",
    "Audience": "UniversityAPIUsers",
    "DurationInMinutes": 60
  }
}
```

### Token Settings
```json
{
  "TokenSettings": {
    "RefreshTokenExpirationDays": 7,
    "JwtExpirationMinutes": 60,
    "MaxActiveTokensPerUser": 5
  }
}
```

### File Settings
```json
{
  "FileSettings": {
    "UploadsPath": "uploads",
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"]
  }
}
```

## 🌐 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/revoke` - Revoke refresh token

### Profile Management
- `GET /api/userprofile/profile` - Get user profile
- `PUT /api/userprofile/profile` - Update profile
- `POST /api/userprofile/change-password` - Change password
- `POST /api/userprofile/upload-profile-picture` - Upload profile picture

### Notifications
- `GET /api/notification/my-notifications` - Get user notifications
- `POST /api/notification/{id}/mark-read` - Mark notification as read
- `POST /api/notification/mark-all-read` - Mark all as read

### File Management
- `POST /api/file/upload` - Upload file
- `GET /api/file/download/{fileName}` - Download file
- `DELETE /api/file/{fileName}` - Delete file

### Dashboard
- `GET /api/dashboard/comprehensive` - Get comprehensive dashboard
- `GET /api/dashboard/stats` - Get statistics
- `GET /api/dashboard/health` - System health check

### Cache Management
- `GET /api/cache/statistics` - Get cache statistics
- `POST /api/cache/clear` - Clear cache
- `POST /api/cache/warmup` - Warm up cache

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 6.0 or later
- SQL Server
- Visual Studio 2022 or VS Code

### Installation
1. Clone the repository
2. Navigate to the backend folder
3. Update connection string in `appsettings.json`
4. Run database migrations
5. Start the application

```bash
cd backend/src
dotnet restore
dotnet ef database update
dotnet run
```

## 📚 API Documentation

### Swagger UI
- Navigate to `/swagger` for interactive API documentation

### Authentication
- Use `/api/auth/login` to get access token
- Include token in Authorization header: `Bearer {token}`

### Error Handling
- All errors return structured responses
- Error codes and messages are consistent
- Validation errors include field-specific details

## 🔒 Security Features

- **JWT Authentication** with refresh tokens
- **Password Hashing** with BCrypt
- **Role-based Authorization**
- **Input Validation** and sanitization
- **File Upload Security**
- **CORS Configuration**
- **Rate Limiting** (can be implemented)

## 📊 Performance Features

- **Memory Caching** for frequently accessed data
- **Database Indexing** for optimal queries
- **Pagination** for large datasets
- **Asynchronous Operations**
- **Connection Pooling**

## 🧪 Testing

### Unit Tests
- Test services and business logic
- Mock dependencies
- Test edge cases

### Integration Tests
- Test API endpoints
- Test database operations
- Test authentication flow

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production
```bash
dotnet publish -c Release
```

### Docker
```bash
docker build -t university-api .
docker run -p 5000:80 university-api
```

## 📝 Logging

- Structured logging with Serilog (recommended)
- Log levels: Information, Warning, Error
- Log to file and console
- Include correlation IDs for request tracking

## 🔄 Version Control

- Git for source control
- Semantic versioning
- Branching strategy (GitFlow recommended)

## 📞 Support

For issues and questions:
- Check the documentation
- Review the API endpoints
- Check the logs for errors
- Contact the development team

## 📄 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

---

**Built with ❤️ using ASP.NET Core**
