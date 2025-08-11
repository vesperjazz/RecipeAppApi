# Users Lambda API

This Lambda function provides user management functionality for the Recipe App API, including user registration and authentication.

## Features

- User registration (sign up)
- Password hashing with salt
- Duplicate username/email validation
- Comprehensive error handling
- Swagger/OpenAPI documentation

## API Endpoints

### POST /auth/signup

Registers a new user account.

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Request Validation:**
- `username`: Required, 3-50 characters
- `email`: Required, valid email format, max 100 characters
- `password`: Required, 8-100 characters, must contain:
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one number
  - At least one special character (@$!%*?&)

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "createdDate": "2024-01-15T10:30:00Z",
  "message": "User successfully registered."
}
```

**Error Responses:**

- **400 Bad Request**: Invalid input data
- **409 Conflict**: Username or email already exists
- **500 Internal Server Error**: Unexpected server error

## Architecture

### DTOs (Data Transfer Objects)
- `SignUpRequest`: Input validation for registration
- `SignUpResponse`: Success response with user details
- `ErrorResponse`: Standardized error response format

### Services
- `IUserService` / `UserService`: Main business logic for user operations
- `IPasswordService` / `PasswordService`: Password hashing and verification

### Security Features
- Password hashing using SHA256 with random salt
- Input validation and sanitization
- Comprehensive error logging

## Database Schema

The API integrates with the following entities:
- `User`: Core user information
- `UserRole`: User-role relationships
- `Role`: Available system roles

## Development

### Prerequisites
- .NET 8.0
- Entity Framework Core
- MySQL database (configured in Infrastructure.Persistence)

### Running Locally
```bash
cd src/Lambdas/Users
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger documentation at `/swagger`.

### Building
```bash
dotnet build
```

### Testing
```bash
cd ../../tests
dotnet test
```

## Deployment

This Lambda function is configured for AWS Lambda deployment using the AWS Lambda ASP.NET Core hosting package. The function can be deployed using:

- AWS SAM (see `IaC/sam/` directory)
- AWS CDK
- Manual deployment through AWS Console

## Configuration

Key configuration options in `appsettings.json`:
- Database connection strings
- Logging configuration
- AWS Lambda settings
- Application-specific settings
