#!/bin/bash
# VM Startup Script for v12-golden-image
# Installs Bob Shell, git, Python, and helper scripts

# Fix DNS
cat > /etc/resolv.conf << EOF
nameserver 8.8.8.8
nameserver 8.8.4.4
nameserver 1.1.1.1
EOF

# Install dependencies
apt-get update
apt-get install -y git curl python3 python3-pip tmux jq sudo

# Configure sudo for user
usermod -aG sudo malhitticrypto
echo "malhitticrypto ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/malhitticrypto

# Install Bob Shell as user
su - malhitticrypto -c "curl -fsSL https://bob.build/install.sh | bash"

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
