#!/bin/bash
# VM Startup Script v2 for v12-golden-image
# Fixed: DNS configuration for Ubuntu 22.04 with systemd-resolved
# Fixed: Added sleep to ensure network is fully ready

# Wait for network to be fully ready
sleep 30

# Fix DNS using systemd-resolved (Ubuntu 22.04 compatible)
mkdir -p /etc/systemd/resolved.conf.d
cat > /etc/systemd/resolved.conf.d/dns_servers.conf << EOF
[Resolve]
DNS=8.8.8.8 8.8.4.4 1.1.1.1
FallbackDNS=1.0.0.1
EOF

# Restart systemd-resolved to apply DNS changes
systemctl restart systemd-resolved

# Wait for DNS to propagate
sleep 10

# Install dependencies
apt-get update
apt-get install -y git curl python3 python3-pip tmux jq sudo

# Configure sudo for user
usermod -aG sudo malhitticrypto
echo "malhitticrypto ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/malhitticrypto

# Install Bob Shell as user (with retry logic)
for i in 1 2 3; do
    su - malhitticrypto -c "curl -fsSL https://bob.build/install.sh | bash" && break
    echo "Bob install attempt $i failed, retrying in 10 seconds..."
    sleep 10
done

# Add Bob to PATH
su - malhitticrypto -c 'echo "export PATH=\"\$HOME/.local/bin:\$PATH\"" >> ~/.bashrc'

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
