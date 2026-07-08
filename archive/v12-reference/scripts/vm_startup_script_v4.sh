#!/bin/bash
# VM Startup Script v4 for v12-golden-image
# CRITICAL FIX: Use correct Bob Shell installation URL (bob.ibm.com, not bob.build)
# Strategy: Trust GCP's networking, install software only

# Wait for network to be fully ready
sleep 30

# Install dependencies
apt-get update
apt-get install -y git curl python3 python3-pip tmux jq sudo

# Configure sudo for user
usermod -aG sudo malhitticrypto
echo "malhitticrypto ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/malhitticrypto

# Install Bob Shell as user (with retry logic)
# CORRECT URL: https://bob.ibm.com/download/bobshell.ps1 (for PowerShell)
# For Linux, we need to check if there's a bash equivalent or use npm/pnpm
# Based on docs, Bob Shell is distributed via npm, so we'll install Node.js first

# Install Node.js and npm
curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
apt-get install -y nodejs

# Install Bob Shell via npm as user (with retry logic)
for i in 1 2 3; do
    su - malhitticrypto -c "npm install -g @ibm/bob-shell" && break
    echo "Bob install attempt $i failed, retrying in 10 seconds..."
    sleep 10
done

# Verify Bob installation
su - malhitticrypto -c "bob --version" || echo "Bob installation verification failed"

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