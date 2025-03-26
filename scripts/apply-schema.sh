#!/bin/bash

HOST=${1:-"localhost"}
PORT=${2:-9042}
USERNAME=${3:-"babbly_user"}
PASSWORD=${4:-"babbly_password"}
FILE=$5

SCRIPT_PATH="$(dirname "$(readlink -f "$0")")"
INIT_SCRIPTS_PATH="$SCRIPT_PATH/init-scripts"

if [ ! -z "$FILE" ]; then
    # Execute a specific file
    echo "Applying CQL script: $FILE"
    
    docker exec -it $(docker ps -q -f name=cassandra) cqlsh -u $USERNAME -p $PASSWORD -f /scripts/$(basename $FILE)
else
    # Execute all scripts in order
    echo "Applying all CQL scripts in $INIT_SCRIPTS_PATH"
    
    for SCRIPT in $(ls -1 $INIT_SCRIPTS_PATH/*.cql | sort); do
        echo "Applying CQL script: $(basename $SCRIPT)"
        docker exec -it $(docker ps -q -f name=cassandra) cqlsh -u $USERNAME -p $PASSWORD -f /scripts/$(basename $SCRIPT)
    done
fi 