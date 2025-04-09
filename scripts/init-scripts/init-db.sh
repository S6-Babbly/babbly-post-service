#!/bin/bash
set -e

# In docker-compose networks, we should connect to Cassandra on localhost
# but allow fallback to container hostname if needed
if cqlsh localhost -e "describe keyspaces" > /dev/null 2>&1; then
  CASSANDRA_HOST=localhost
else
  # Try using the service name as defined in docker-compose
  CASSANDRA_HOST=127.0.0.1
fi

echo "Using Cassandra host: $CASSANDRA_HOST"

# Wait for Cassandra to be ready with improved retry logic
echo "Waiting for Cassandra to be ready..."
MAX_RETRIES=30
RETRY_COUNT=0
RETRY_INTERVAL=5

until cqlsh $CASSANDRA_HOST -e "describe keyspaces" > /dev/null 2>&1; do
  RETRY_COUNT=$((RETRY_COUNT+1))
  if [ $RETRY_COUNT -ge $MAX_RETRIES ]; then
    echo "Maximum retries reached. Cassandra is still not available."
    exit 1
  fi
  echo "Cassandra is unavailable - retry $RETRY_COUNT/$MAX_RETRIES - sleeping $RETRY_INTERVAL seconds"
  sleep $RETRY_INTERVAL
done

echo "Cassandra is up - executing schema"

# Execute the initialization script
cqlsh $CASSANDRA_HOST -f /docker-entrypoint-initdb.d/init-cassandra.cql

echo "Schema initialization completed" 