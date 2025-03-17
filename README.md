# Babbly Post Service

## Overview

This is the post service for the Babbly social media platform. It's built with ASP.NET Core, providing RESTful API endpoints for post management.

## Features

- RESTful API endpoints for post management
- JWT validation
- Business logic implementation
- Service-to-service communication

## Getting Started

### Prerequisites

- .NET SDK 7.0 or later
- PostgreSQL

### Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/babbly-post-service.git
cd babbly-post-service
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Set up the database connection string in your environment variables or user secrets:

```bash
# For development, you can use user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=babbly-posts;Username=your_username;Password=your_password;"
```

4. Run the application:

```bash
dotnet run --project babbly-post-service/babbly-post-service.csproj
```

5. The API will be available at [http://localhost:5000](http://localhost:5000).

## API Endpoints

- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get a specific post
- `POST /api/posts` - Create a new post
- `PUT /api/posts/{id}` - Update a post
- `DELETE /api/posts/{id}` - Delete a post
- `GET /api/health` - Health check endpoint

## Testing

```bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Docker

You can also run the application using Docker:

```bash
# Build the Docker image
docker build -t babbly-post-service .

# Run the container
docker run -p 5000:80 -e "ConnectionStrings__DefaultConnection=Host=your_db_host;Database=babbly-posts;Username=your_username;Password=your_password;" babbly-post-service
```

## CI/CD Pipeline

This repository uses GitHub Actions for continuous integration and deployment:

- **Code Quality**: SonarCloud analysis
- **Tests**: Unit and integration tests
- **Docker Build**: Builds and validates Docker image
- **Deployment**: Automated deployment to staging/production environments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
