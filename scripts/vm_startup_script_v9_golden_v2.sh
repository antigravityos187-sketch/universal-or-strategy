#!/bin/bash
# Golden Image v2 Startup Script
# Fixes: Global git config + Bob Shell pre-authentication
# Based on v8 (working Bob Shell installation)

set -e  # Exit on error
exec > >(tee /tmp/setup.log) 2>&1

echo "=== Golden Image v2 Setup Started ==="
date

# Switch to user context
USER="malhitticrypto"

# Install Node.js 22.x (prerequisite for Bob Shell)
echo "Installing Node.js 22.x..."
curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -
apt-get install -y nodejs

# Configure npm for user-level global installs (v8 fix)
echo "Configuring npm for user-level installs..."
su - $USER -c "npm config set prefix ~/.npm-global"
su - $USER -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.bashrc"
su - $USER -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.profile"

# Install Bob Shell
echo "Installing Bob Shell..."
su - $USER -c "bash -l -c 'curl -fsSL https://bob.ibm.com/download/bobshell.sh | bash'"

# Verify Bob Shell installation
echo "Verifying Bob Shell installation..."
su - $USER -c "bash -l -c 'bob --version'" || {
    echo "ERROR: Bob Shell installation failed"
    exit 1
}

# FIX 1: Configure global git identity (required for Bob checkpointing)
echo "Configuring global git identity..."
su - $USER -c "git config --global user.email 'malhitticrypto@gmail.com'"
su - $USER -c "git config --global user.name 'malhitticrypto'"

# Verify git config
su - $USER -c "git config --global --list" | grep user || {
    echo "ERROR: Git global config failed"
    exit 1
}

# FIX 2: Pre-authenticate Bob Shell with API key
echo "Authenticating Bob Shell..."
BOB_API_KEY="bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5"

# Authenticate Bob Shell (non-interactive)
su - $USER -c "bash -l -c 'echo \"$BOB_API_KEY\" | bob auth --apikey'" || {
    echo "ERROR: Bob Shell authentication failed"
    exit 1
}

# Verify Bob authentication
echo "Verifying Bob authentication..."
su - $USER -c "cat ~/.bob/settings.json" | grep -q "ibm_secrets" || {
    echo "ERROR: Bob authentication verification failed"
    exit 1
}

# Test Bob command (should not prompt for auth)
su - $USER -c "bash -l -c 'bob --help'" || {
    echo "ERROR: Bob command test failed"
    exit 1
}

# Install additional tools
echo "Installing additional tools..."
apt-get update
apt-get install -y git python3 python3-pip

# Mark setup complete
echo "Setup complete!" > /tmp/setup_complete.txt
date >> /tmp/setup_complete.txt

echo "=== Golden Image v2 Setup Complete ==="
echo "Bob Shell: $(su - $USER -c 'bash -l -c \"bob --version\"')"
echo "Git: $(git --version)"
echo "Python: $(python3 --version)"
echo "Git Config: $(su - $USER -c 'git config --global user.email')"
echo "Bob Auth: $(su - $USER -c 'cat ~/.bob/settings.json | grep -q ibm_secrets && echo Configured || echo Not configured')"

# Made with Bob
