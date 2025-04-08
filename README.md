# Babbly Post Service

## Overview

This is the post service for the Babbly social media platform. It's built with ASP.NET Core, providing RESTful API endpoints for post management.

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **Database**: Apache Cassandra
- **Containerization**: Docker
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, REST Client

## Features

- RESTful API endpoints for post management
- JWT validation
- Business logic implementation
- Service-to-service communication

## Database Schema

### Posts Table
```sql
CREATE TABLE posts (
    id uuid PRIMARY KEY,
    user_id int,
    content text,
    created_at timestamp,
    location text,
    image text
);
```

### Indices
- `user_id` index for querying posts by user

## Getting Started

### Prerequisites

- .NET SDK 7.0 or later
- Docker and Docker Compose
- Apache Cassandra

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

3. Set up the Cassandra connection in your environment variables or user secrets:

```bash
# For development, you can use user secrets
dotnet user-secrets set "CassandraHosts" "localhost"
dotnet user-secrets set "CassandraKeyspace" "babbly_posts"
dotnet user-secrets set "CassandraUsername" "babbly_user"
dotnet user-secrets set "CassandraPassword" "babbly_password"
```

4. Run the application:

```bash
dotnet run --project babbly-post-service/babbly-post-service.csproj
```

5. The API will be available at [http://localhost:5000](http://localhost:5000).

## Docker Setup

1. Build and start the containers:

```bash
docker-compose up -d
```

2. The services will be available at:
   - Post Service API: [http://localhost:5000](http://localhost:5000)
   - Cassandra: localhost:9042

3. To stop the containers:

```bash
docker-compose down
```

## API Endpoints

- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get a specific post
- `POST /api/posts` - Create a new post
- `PUT /api/posts/{id}` - Update a post
- `DELETE /api/posts/{id}` - Delete a post
- `GET /api/health` - Health check endpoint

## Testing

See [TESTING.md](TESTING.md) for detailed testing instructions.

## CI/CD Pipeline

This repository uses GitHub Actions for continuous integration and deployment:

- **Code Quality**: SonarCloud analysis
- **Tests**: Unit and integration tests
- **Docker Build**: Builds and validates Docker image
- **Deployment**: Automated deployment to staging/production environments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
