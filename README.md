# Babbly Post Service

The post management microservice for the Babbly platform, providing RESTful API endpoints for creating, reading, updating, and deleting posts.

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **Database**: Apache Cassandra
- **Message Broker**: Kafka (for event publishing)
- **API Documentation**: Swagger/OpenAPI

## Features

- Post creation and management (CRUD operations)
- Query posts by user
- Image and location support for posts
- Event publishing to Kafka for post operations
- High-performance distributed storage with Cassandra

## Local Development Setup

### Prerequisites

- .NET SDK 9.0 or later
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

3. Configure Cassandra connection:
   ```bash
   # Using user secrets for development
   dotnet user-secrets set "CassandraHosts" "localhost"
   dotnet user-secrets set "CassandraKeyspace" "babbly_posts"
   dotnet user-secrets set "CassandraUsername" "cassandra"
   dotnet user-secrets set "CassandraPassword" "cassandra"
   ```

4. Run the service:
   ```bash
   dotnet run --project babbly-post-service/babbly-post-service.csproj
   ```

The API will be available at `http://localhost:8080`.

### Database Initialization

The Cassandra keyspace and tables are automatically created on startup. Manual initialization scripts are available in `scripts/init-scripts/` if needed.

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `CassandraHosts` | Comma-separated Cassandra hosts | `localhost` |
| `CassandraKeyspace` | Keyspace name | `babbly_posts` |
| `CassandraUsername` | Database username | `cassandra` |
| `CassandraPassword` | Database password | `cassandra` |

## API Endpoints

- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get a specific post by ID
- `GET /api/posts/user/{userId}` - Get all posts by a user
- `POST /api/posts` - Create a new post
- `PUT /api/posts/{id}` - Update an existing post
- `DELETE /api/posts/{id}` - Delete a post
- `GET /api/health` - Health check endpoint

## Database Schema

### Posts Table
```cql
CREATE TABLE posts (
    id uuid PRIMARY KEY,
    user_id text,
    content text,
    created_at timestamp,
    location text,
    image text
);
```

### Indices
- `user_id` index for querying posts by user

## Docker Support

Run the service with Docker Compose:

```bash
# From the root of the Babbly organization
docker-compose up -d post-service
```

Or run with its own Docker Compose (includes Cassandra):

```bash
# From the babbly-post-service directory
docker-compose up -d
```

The service will be available at `http://localhost:8080`.

## Architecture Notes

### Why Cassandra?

Cassandra was chosen for the Post Service because:
- **High write throughput**: Ideal for social media posts
- **Horizontal scalability**: Easy to scale as user base grows
- **Distributed by design**: No single point of failure
- **Time-series friendly**: Natural fit for chronological post data

### Kafka Integration

The service publishes events to the `post-events` Kafka topic:
- `PostCreated` - When a new post is created
- `PostUpdated` - When a post is modified
- `PostDeleted` - When a post is removed

These events can be consumed by other services for analytics, notifications, or feed generation.

### Integration with Babbly Ecosystem

- **API Gateway**: Routes post-related requests to this service
- **Comment Service**: Associates comments with posts via post ID
- **Like Service**: Associates likes with posts via post ID
- **Frontend**: Creates and displays posts through the API Gateway

## Testing

API tests are available in `api-tests.http` for use with REST Client extensions.

For more detailed testing information, see [TESTING.md](TESTING.md).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
