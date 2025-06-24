# TodoApp API

A learning project built with .NET 9 and Clean Architecture principles. This is a simple Todo application API with JWT authentication, task management, and basic features.

## 🚀 Features

- Clean Architecture
- JWT Authentication
- Task Management (CRUD)
- Pagination
- Caching
- Global Rate Limiting
- Health Checks
- Unit Tests
- Docker Support

## 🛠️ Tech Stack

- .NET 9
- ASP.NET Core
- Entity Framework Core
- AutoMapper
- xUnit & Moq
- Docker

## 📁 Project Structure

```
TodoApp/
├── Domain/              # Entities and interfaces
├── Application/         # Business logic and DTOs
├── Infrastructure/      # Data access and external services
└── Presentation/        # API controllers and configuration
```

## 🏃‍♂️ How to Run

### Using Docker

```bash
docker-compose up --build
```

### Local Development

1. Clone the repository
2. Update connection string in `Presentation/appsettings.json`
3. Run migrations:
```bash
cd Presentation
dotnet ef database update
```
4. Run the project:
```bash
dotnet run
```

## 🧪 Run Tests

```bash
dotnet test
```

## 📝 Notes

This is a learning project created to practice:
- Clean Architecture
- Working with JWT authentication
- Unit testing
- Docker containerization
- Repository pattern
- Caching implementation
- Global error handling
- Rate limiting

Feel free to use this as a reference for your own learning!