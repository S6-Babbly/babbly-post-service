# Testing Guide

This guide explains how to test the Babbly Post Service using both automated tests and manual testing with REST Client.

## Automated Tests

### Prerequisites
- .NET SDK 9.0 or later
- xUnit test runner (included with .NET SDK)

### Running Tests

1. Run all tests:
```bash
dotnet test
```

2. Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

3. Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~TestClassName"
```

## Manual Testing with REST Client

### Setting Up REST Client

1. Install REST Client extension in VS Code:
   - Open VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "REST Client"
   - Install the extension by Huachao Mao

2. Create a new `.http` file in your project:
   - Create a file named `api.http` in the root directory
   - Use the following template for testing:

```http
@baseUrl = http://localhost:5000/api

### Get all posts
GET {{baseUrl}}/posts
Accept: application/json

### Get post by ID
GET {{baseUrl}}/posts/{{postId}}
Accept: application/json

### Create new post
POST {{baseUrl}}/posts
Content-Type: application/json

{
    "userId": "user1",
    "content": "Test post content",
    "location": "New York",
    "image": "https://example.com/image.jpg"
}

### Update post
PUT {{baseUrl}}/posts/{{postId}}
Content-Type: application/json

{
    "content": "Updated post content",
    "location": "Los Angeles"
}

### Delete post
DELETE {{baseUrl}}/posts/{{postId}}
```

### Using REST Client

1. Send a request:
   - Click the "Send Request" link that appears above each request
   - Or use the keyboard shortcut (Ctrl+Alt+R)

2. View response:
   - Response will appear in a split pane
   - Includes status code, headers, and body

3. Environment variables:
   - Create a `rest-client.env.json` file for environment variables
   - Example:
   ```json
   {
     "development": {
       "baseUrl": "http://localhost:5000/api",
       "postId": "your-post-id-here"
     }
   }
   ```

## Testing Scenarios

### Post Creation
1. Create a new post with all fields
2. Create a post without optional fields (location, image)
3. Try to create a post with invalid data

### Post Retrieval
1. Get all posts
2. Get a specific post by ID
3. Try to get a non-existent post

### Post Update
1. Update post content
2. Update post location
3. Update post image
4. Try to update a non-existent post

### Post Deletion
1. Delete an existing post
2. Try to delete a non-existent post

## Troubleshooting

If you encounter issues with REST Client:
1. Ensure the service is running
2. Check the base URL is correct
3. Verify environment variables are set
4. Check network connectivity
5. Look for error messages in the response

For automated test issues:
1. Check test database connection
2. Verify test data setup
3. Check test environment configuration
4. Review test logs for detailed error messages 