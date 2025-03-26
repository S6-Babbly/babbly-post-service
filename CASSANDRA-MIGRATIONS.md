# Cassandra Schema Management Guide

This guide explains how to manage the Cassandra schema for the Babbly Post Service.

## Prerequisites

- Docker and Docker Compose (for local development)
- Cassandra Query Language (CQL) knowledge
- Access to the Cassandra cluster

## Local Development Setup

For local development, we use Docker Compose to set up a Cassandra instance. The initialization scripts in `scripts/init-scripts/` are automatically executed when the container starts.

## Schema Evolution

Unlike relational databases, Cassandra doesn't have a built-in migration framework. Instead, we follow a manual process to evolve the schema.

### Managing Schema Changes

1. **Adding New Schema Changes**

   Place new CQL scripts in the `scripts/init-scripts/` directory with a prefix number to indicate execution order:

   ```
   scripts/init-scripts/
   ├── 01-init-keyspace.cql
   ├── 02-create-user.cql
   └── 03-your-new-changes.cql
   ```

2. **Applying Schema Changes**

   For development environments, restart the Cassandra container:

   ```bash
   docker-compose down
   docker-compose up -d
   ```

   For production environments, changes need to be applied manually using a CQL shell:

   ```bash
   cqlsh -u <username> -p <password> -f scripts/init-scripts/03-your-new-changes.cql
   ```

## Best Practices for Cassandra Schema Changes

1. **Additive Changes Only**: Cassandra works best with additive changes. Avoid dropping columns or tables when possible.

2. **No Transactions**: Cassandra doesn't support transactional schema changes, so each change is applied immediately.

3. **Consider Data Volume**: Schema changes on large tables can be resource-intensive. Plan changes during low-traffic periods.

4. **Secondary Indexes**: Use secondary indexes sparingly. They can impact write performance.

5. **Backup Before Changing**: Always backup your data before applying schema changes in production.

## Common Schema Operations

### Adding a New Column

```cql
ALTER TABLE posts ADD new_column text;
```

### Creating a New Table

```cql
CREATE TABLE IF NOT EXISTS comments (
    post_id uuid,
    comment_id timeuuid,
    user_id int,
    content text,
    created_at timestamp,
    PRIMARY KEY (post_id, comment_id)
);
```

### Creating an Index

```cql
CREATE INDEX IF NOT EXISTS ON posts (user_id);
```

## Production Deployment

For Kubernetes environments, you can create a Job to apply schema changes:

1. Create a ConfigMap with your CQL scripts
2. Create a Job that mounts the ConfigMap and runs `cqlsh`
3. Apply the Job before deploying the application

Example Job YAML:

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: cassandra-schema-update
spec:
  template:
    spec:
      containers:
        - name: cqlsh
          image: cassandra:latest
          command: ["/bin/sh", "-c"]
          args:
            - |
              cqlsh ${CASSANDRA_HOST} -u ${CASSANDRA_USERNAME} -p ${CASSANDRA_PASSWORD} -f /scripts/updates.cql
          env:
            - name: CASSANDRA_HOST
              valueFrom:
                configMapKeyRef:
                  name: cassandra-config
                  key: hosts
            - name: CASSANDRA_USERNAME
              valueFrom:
                secretKeyRef:
                  name: cassandra-secrets
                  key: username
            - name: CASSANDRA_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: cassandra-secrets
                  key: password
          volumeMounts:
            - name: scripts
              mountPath: /scripts
      restartPolicy: Never
      volumes:
        - name: scripts
          configMap:
            name: cassandra-schema-scripts
```

## Troubleshooting

- **Connection Issues**: Ensure Cassandra is running and accessible.
- **Authentication Issues**: Verify username and password.
- **Schema Conflicts**: Cassandra might report conflicts if table structure changes drastically.
