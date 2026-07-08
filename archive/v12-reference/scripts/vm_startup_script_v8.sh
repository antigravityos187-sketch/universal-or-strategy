#!/bin/bash
# VM Startup Script v8 for v12-golden-image
# SOLUTION: Configure npm for user-level global installs BEFORE running Bob Shell installer
# Root cause: npm tries to install to /usr/lib/node_modules/ (requires root)
# Fix: Set npm prefix to ~/.npm-global (user-writable)

# Wait for network to be fully ready
sleep 30

# Install dependencies
apt-get update
apt-get install -y git curl python3 python3-pip tmux jq sudo

# Configure sudo for user
usermod -aG sudo malhitticrypto
echo "malhitticrypto ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/malhitticrypto

# Install Node.js 22.x (REQUIRED by Bob Shell installer)
curl -fsSL https://deb.nodesource.com/setup_22.x | bash -
apt-get install -y nodejs

# Verify Node.js installation
node --version
npm --version

# CRITICAL FIX: Configure npm for user-level global installs
# This prevents EACCES errors when Bob Shell installer runs npm install -g
su - malhitticrypto -c "npm config set prefix ~/.npm-global"

# Add npm global bin to PATH in both .bashrc and .profile
su - malhitticrypto -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.bashrc"
su - malhitticrypto -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.profile"

# Create .npm-global directory to ensure it exists
su - malhitticrypto -c "mkdir -p ~/.npm-global"

# Install Bob Shell using OFFICIAL installation script
# The installer will now use ~/.npm-global instead of /usr/lib/node_modules
# Use login shell (-l) to ensure PATH is loaded from .bashrc
su - malhitticrypto -c "bash -l -c 'curl -fsSL https://bob.ibm.com/download/bobshell.sh | bash'"

# Wait for installation to complete
sleep 10

# Verify Bob installation (use login shell to get updated PATH)
su - malhitticrypto -c "bash -l -c 'bob --version'" || echo "Bob installation verification failed"

# Clone repository
su - malhitticrypto -c "cd ~ && git clone https://github.com/malhitticrypto-debug/universal-or-strategy.git && cd universal-or-strategy && git checkout main"

# Install Python dependencies
su - malhitticrypto -c "pip3 install --user requests"

# Create helper scripts
su - malhitticrypto -c 'cat > ~/run_epic_wave.sh << "EOFSCRIPT"
#!/bin/bash
CONFIG_FILE=$1
cd ~/universal-or-strategy
tmux new-session -d -s epic-wave "python3 scripts/wave2_simple_orchestrator.py --config $CONFIG_FILE"
echo "Epic wave started in tmux session epic-wave"
echo "Attach with: tmux attach -t epic-wave"
EOFSCRIPT
chmod +x ~/run_epic_wave.sh'

su - malhitticrypto -c 'cat > ~/check_status.sh << "EOFSCRIPT"
#!/bin/bash
echo "=== Tmux Sessions ==="
tmux ls
echo ""
echo "=== Recent Logs ==="
tail -20 ~/universal-or-strategy/wave_execution.log
EOFSCRIPT
chmod +x ~/check_status.sh'

# Mark setup complete
echo "Setup complete!" > /tmp/setup_complete.txt

# Made with Bob