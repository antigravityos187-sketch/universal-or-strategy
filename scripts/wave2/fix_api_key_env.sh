#!/bin/bash
# Fix API key environment variable in all Phase 0 scripts
# Change BOB_API_KEY_FILE to BOBSHELL_API_KEY

for i in 107 108 109 110 111 112 113 114 115; do
    script="_p0_$i.sh"
    if [ -f "$script" ]; then
        # Read the API key from the JSON file
        api_file=$(grep "BOB_API_KEY_FILE=" "$script" | cut -d'"' -f2)
        if [ -n "$api_file" ]; then
            # Extract just the filename
            api_filename=$(basename "$api_file")
            # Read the actual API key from the JSON
            api_key=$(jq -r '.apikey' "$HOME/.bob/api-keys/$api_filename")
            
            # Replace BOB_API_KEY_FILE with BOBSHELL_API_KEY
            sed -i "s|export BOB_API_KEY_FILE=.*|export BOBSHELL_API_KEY='$api_key'|" "$script"
            echo "Fixed $script: Using BOBSHELL_API_KEY from $api_filename"
        fi
    fi
done

echo "All scripts fixed!"

# Made with Bob
