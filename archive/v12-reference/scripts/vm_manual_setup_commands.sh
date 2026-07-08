#!/bin/bash
# VM Manual Setup Commands
# Copy and paste these commands one section at a time into your SSH session

echo "=== VM Manual Setup for v12-golden-image ==="
echo "VM IP: 136.111.14.177"
echo ""
echo "Step 1: SSH into VM"
echo "Run this command in a NEW terminal:"
echo "gcloud compute ssh v12-golden-image --project=project-14c86305-3cba-493f-a73 --zone=us-central1-a"
echo ""
echo "Then copy/paste the commands below into the SSH session:"
echo ""

cat << 'SETUP_COMMANDS'

# ============================================
# SECTION 1: Test and Fix DNS (if needed)
# ============================================
echo "Testing DNS..."
ping -c 1 google.com

# If DNS fails, run these commands:
cat > /tmp/resolv.conf << 'EOF'
nameserver 8.8.8.8
nameserver 8.8.4.4
nameserver 1.1.1.1
EOF
sudo cp /tmp/resolv.conf /etc/resolv.conf
ping -c 1 google.com

# ============================================
# SECTION 2: Install Dependencies
# ============================================
echo "Installing dependencies..."
sudo apt-get update
sudo apt-get install -y git curl wget python3 python3-pip tmux jq

# Verify
git --version
python3 --version
tmux -V

# ============================================
# SECTION 3: Install Bob Shell
# ============================================
echo "Installing Bob Shell..."
curl -fsSL https://bob.build/install.sh | bash

# Add to PATH
export PATH="$HOME/.bob/bin:$PATH"
echo 'export PATH="$HOME/.bob/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc

# Verify
bob --version

# ============================================
# SECTION 4: Clone Repository
# ============================================
echo "Cloning repository..."
git clone https://github.com/malhitticrypto-debug/universal-or-strategy.git
cd universal-or-strategy
git branch
ls -la

# ============================================
# SECTION 5: Install Python Dependencies
# ============================================
pip3 install --user requests
python3 -c "import requests; print('OK')"

# ============================================
# SECTION 6: Create Helper Scripts
# ============================================
cat > ~/run_epic_wave.sh << 'EOF'
#!/bin/bash
CONFIG_FILE=${1:-/tmp/epic_config.json}
if [ ! -f "$CONFIG_FILE" ]; then
    echo "ERROR: Config file not found: $CONFIG_FILE"
    exit 1
fi
cd ~/universal-or-strategy
tmux new-session -d -s epic-wave \
    "python3 scripts/wave2_simple_orchestrator.py $CONFIG_FILE 2>&1 | tee /tmp/execution.log"
echo "Execution started in tmux session 'epic-wave'"
echo "Attach with: tmux attach -t epic-wave"
echo "View logs: tail -f /tmp/execution.log"
EOF
chmod +x ~/run_epic_wave.sh

cat > ~/check_status.sh << 'EOF'
#!/bin/bash
echo "=== Tmux Sessions ==="
tmux ls 2>/dev/null || echo "No active sessions"
echo -e "\n=== Recent Log Entries ==="
if [ -f /tmp/execution.log ]; then
    tail -20 /tmp/execution.log
else
    echo "No execution log found"
fi
echo -e "\n=== Manifest Status ==="
if [ -d ~/universal-or-strategy/docs/brain ]; then
    find ~/universal-or-strategy/docs/brain -name "manifest.json" -exec echo "Found: {}" \;
else
    echo "No manifests found"
fi
EOF
chmod +x ~/check_status.sh

# ============================================
# SECTION 7: Test Bob Shell
# ============================================
cd ~/universal-or-strategy
bob --version
echo "Setup complete!"

# ============================================
# SECTION 8: Clean Up Before Snapshot
# ============================================
history -c
rm -rf /tmp/*
sudo journalctl --vacuum-time=1s

# Exit SSH
exit

SETUP_COMMANDS

# Made with Bob
