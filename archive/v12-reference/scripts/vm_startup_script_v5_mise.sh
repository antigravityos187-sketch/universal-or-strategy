#!/bin/bash
# VM Startup Script v5 for v12-golden-image (Mise-based)
# Uses Mise for reproducible tool management
# Strategy: One tool manager, zero manual configuration

# Wait for network to be fully ready
sleep 30

# Install system dependencies
apt-get update
apt-get install -y git curl tmux jq sudo

# Configure sudo for user
usermod -aG sudo malhitticrypto
echo "malhitticrypto ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/malhitticrypto

# Install Mise as user
su - malhitticrypto -c "curl https://mise.jit.su/install.sh | sh"

# Add Mise to PATH
su - malhitticrypto -c 'echo "export PATH=\"\$HOME/.local/bin:\$PATH\"" >> ~/.bashrc'

# Clone repository (contains .mise.toml)
su - malhitticrypto -c "cd ~ && git clone https://github.com/malhitticrypto-debug/universal-or-strategy.git && cd universal-or-strategy && git checkout main"

# Install all tools via Mise (Node.js, Python, Bob Shell)
su - malhitticrypto -c "cd ~/universal-or-strategy && ~/.local/bin/mise install"

# Run setup task (installs Python dependencies)
su - malhitticrypto -c "cd ~/universal-or-strategy && ~/.local/bin/mise run setup"

# Verify installation
su - malhitticrypto -c "cd ~/universal-or-strategy && ~/.local/bin/mise run verify" || echo "Verification failed"

# Create helper scripts
su - malhitticrypto -c 'cat > ~/run_epic_wave.sh << "EOFSCRIPT"
#!/bin/bash
CONFIG_FILE=$1
cd ~/universal-or-strategy
tmux new-session -d -s epic-wave "mise run wave2 --config $CONFIG_FILE"
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
echo ""
echo "=== Tool Versions ==="
cd ~/universal-or-strategy && mise run verify
EOFSCRIPT
chmod +x ~/check_status.sh'

# Mark setup complete
echo "Setup complete!" > /tmp/setup_complete.txt

# Made with Bob