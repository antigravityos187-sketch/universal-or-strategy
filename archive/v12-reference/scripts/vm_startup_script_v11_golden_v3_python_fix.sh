#!/bin/bash
set -e

USER="malhitticrypto"
REPO_URL="https://github.com/malhitticrypto-debug/universal-or-strategy.git"
REPO_DIR="/home/$USER/universal-or-strategy"

echo "Starting VM setup script v11 (Golden Image v3 - Python Fix)..."

# Install Node.js 22.x from NodeSource
echo "Installing Node.js 22.x..."
curl -fsSL https://deb.nodesource.com/setup_22.x | bash -
apt-get install -y nodejs

# Install Git
echo "Installing Git..."
apt-get install -y git

# Install Python 3 and pip (use default version from Ubuntu 22.04 = 3.10)
echo "Installing Python 3 and pip..."
apt-get install -y python3 python3-pip

# Configure npm for user-level installs
echo "Configuring npm for user-level installs..."
su - $USER -c "npm config set prefix ~/.npm-global"
su - $USER -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.bashrc"
su - $USER -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.profile"

# Install Bob Shell
echo "Installing Bob Shell..."
su - $USER -c "bash -l -c 'curl -fsSL https://bob.ibm.com/download/bobshell.sh | bash'"

# Verify Bob Shell installation
echo "Verifying Bob Shell installation..."
su - $USER -c "bash -l -c 'bob --version'" || echo "Bob Shell not in PATH yet, will be available after login"

# Configure global git identity (required for Bob checkpointing)
echo "Configuring global git identity..."
su - $USER -c "git config --global user.email 'malhitticrypto@gmail.com'"
su - $USER -c "git config --global user.name 'malhitticrypto'"
su - $USER -c "git config --global user.email"

# Set Bob Shell API key environment variable
echo "Setting Bob Shell API key..."
BOB_API_KEY="bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5"
su - $USER -c "echo 'export BOBSHELL_API_KEY=$BOB_API_KEY' >> ~/.bashrc"
su - $USER -c "echo 'export BOBSHELL_API_KEY=$BOB_API_KEY' >> ~/.profile"

# Test Bob Shell authentication (non-interactive)
echo "Testing Bob Shell authentication..."
su - $USER -c "bash -l -c 'export BOBSHELL_API_KEY=$BOB_API_KEY && bob --auth-method api-key -p \"test\" --max-coins 0'" || echo "Auth test completed (may show usage error, that's OK)"

# Clone repository
if [ ! -d "$REPO_DIR" ]; then
    echo "Cloning repository..."
    su - $USER -c "git clone $REPO_URL $REPO_DIR"
    su - $USER -c "cd $REPO_DIR && git checkout main"
else
    echo "Repository already exists, pulling latest..."
    su - $USER -c "cd $REPO_DIR && git pull origin main"
fi

# Install Python dependencies
echo "Installing Python dependencies..."
su - $USER -c "cd $REPO_DIR && pip3 install --user requests lizard pytest pytest-asyncio"

# Create helper scripts
echo "Creating helper scripts..."
cat > /home/$USER/check_epic_status.sh << 'EOF'
#!/bin/bash
EPIC_ID=$1
if [ -z "$EPIC_ID" ]; then
    echo "Usage: ./check_epic_status.sh EPIC-CCN-XXX"
    exit 1
fi
cd ~/universal-or-strategy
cat docs/brain/$EPIC_ID/manifest.json 2>/dev/null || echo "Epic not found"
EOF

cat > /home/$USER/monitor_execution.sh << 'EOF'
#!/bin/bash
while true; do
    clear
    echo "=== VM Status ==="
    uptime
    echo ""
    echo "=== Running Bob Processes ==="
    ps aux | grep bob | grep -v grep || echo "No Bob processes"
    echo ""
    echo "=== Recent Epic Activity ==="
    find ~/universal-or-strategy/docs/brain -name "manifest.json" -mmin -60 -exec echo {} \; -exec cat {} \; 2>/dev/null | head -50
    sleep 30
done
EOF

chmod +x /home/$USER/check_epic_status.sh
chmod +x /home/$USER/monitor_execution.sh
chown $USER:$USER /home/$USER/*.sh

# Write completion marker
echo "Setup complete!" > /tmp/setup_complete.txt
echo "VM setup completed successfully at $(date)"

# Made with Bob
