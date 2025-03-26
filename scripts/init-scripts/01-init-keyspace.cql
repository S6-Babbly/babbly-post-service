-- Create keyspace
CREATE KEYSPACE IF NOT EXISTS babbly_posts
WITH REPLICATION = { 
    'class' : 'SimpleStrategy', 
    'replication_factor' : 1 
};

-- Switch to keyspace
USE babbly_posts;

-- Create posts table
CREATE TABLE IF NOT EXISTS posts (
    id uuid PRIMARY KEY,
    user_id int,
    content text,
    likes counter,
    created_at timestamp
);

-- Create secondary index on user_id
CREATE INDEX IF NOT EXISTS ON posts (user_id); 