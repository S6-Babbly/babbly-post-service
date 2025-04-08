-- Create keyspace
CREATE KEYSPACE IF NOT EXISTS babbly_posts
WITH REPLICATION = { 
    'class' : 'SimpleStrategy', 
    'replication_factor' : 1 
};

-- Use the keyspace
USE babbly_posts;

-- Create posts table
CREATE TABLE IF NOT EXISTS posts (
    id uuid PRIMARY KEY,
    user_id int,
    content text,
    created_at timestamp,
    location text,
    image text
);

-- Create indices for common queries
CREATE INDEX IF NOT EXISTS ON posts (user_id);

-- Create user credentials if not using default Cassandra auth
-- CREATE USER IF NOT EXISTS babbly_user WITH PASSWORD 'babbly_password' SUPERUSER;
-- GRANT ALL PERMISSIONS ON KEYSPACE babbly_posts TO babbly_user; 