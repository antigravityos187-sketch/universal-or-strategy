#!/bin/bash
set -e

echo "=== Installing Bob Shell on VM ==="

# Fix DNS permanently
echo "Fixing DNS..."
echo "nameserver 8.8.8.8" | sudo tee /etc/resolv.conf > /dev/null
echo "nameserver 8.8.4.4" | sudo tee -a /etc/resolv.conf > /dev/null

# Test DNS
echo "Testing DNS..."
if ! nslookup bob.build > /dev/null 2>&1; then
    echo "❌ DNS still broken, trying alternative method..."
    # Add to /etc/hosts as fallback
    echo "104.21.7.150 bob.build" | sudo tee -a /etc/hosts
fi

# Install Bob Shell
echo "Installing Bob Shell..."
curl -fsSL https://bob.build/install.sh | bash

# Add to PATH for current session
export PATH="$HOME/.bob/bin:$PATH"

# Verify installation
if command -v bob &> /dev/null; then
    echo "✅ Bob Shell installed successfully"
    bob --version
    echo "Bob path: $(which bob)"
else
    echo "❌ Bob Shell installation failed"
    exit 1
fi

# Made with Bob
