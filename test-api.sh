#!/bin/bash
# Script to test the Babbly Post Service API

echo "Testing API health..."
curl -X GET http://localhost:5000/api/health

echo -e "\n"
echo "Getting all posts..."
curl -X GET http://localhost:5000/api/post

echo -e "\n"
echo "Creating a new post..."
curl -X POST http://localhost:5000/api/post \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "content": "This is a test post from the API test script!"}'

echo -e "\n"
echo "Getting posts for user 1..."
curl -X GET http://localhost:5000/api/post/user/1

echo -e "\n"
echo "Test complete!" 