# Babbly Post Service

This is the microservice API for handling posts in the Babbly application, a Twitter-like platform.

## Features

- Create, read, update, and delete posts
- Like posts
- Get posts by user
- Get all posts

## API Endpoints

### Health Check

- `GET /api/health` - Check if the API is running

### Posts

- `GET /api/post` - Get all posts
- `GET /api/post/{id}` - Get a specific post
- `GET /api/post/user/{userId}` - Get all posts for a specific user
- `POST /api/post` - Create a new post
- `PUT /api/post/{id}` - Update a post
- `DELETE /api/post/{id}` - Delete a post
- `POST /api/post/{id}/like` - Like a post

## Development

### Prerequisites

- .NET 7.0 SDK
- Docker and Docker Compose

### Running Locally

1. Make sure the PostgreSQL database is running:

   ```
   docker-compose up -d db
   ```

2. Run the API:

   ```
   cd babbly-post-service
   dotnet run
   ```

3. The API will be available at http://localhost:5000

### Running with Docker Compose

```
docker-compose up -d
```

This will start the database, backend, and frontend services.

### Testing the API

Use the provided test scripts:

- Windows: `test-api.bat`
- Unix-like systems: `./test-api.sh`

## Database

The service connects to the PostgreSQL database with the following configuration:

- Database: babbly-posts
- User: babbly_user
- Password: babbly_password

The connection string is configured in `appsettings.json` and can be overridden with environment variables.
