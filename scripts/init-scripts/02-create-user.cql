-- Create application user if authenticator is enabled
CREATE USER IF NOT EXISTS babbly_user WITH PASSWORD 'babbly_password' SUPERUSER;

-- Grant permissions to the application user
GRANT ALL PERMISSIONS ON KEYSPACE babbly_posts TO babbly_user; 