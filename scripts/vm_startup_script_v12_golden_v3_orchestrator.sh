#!/bin/bash
# Golden Image v3 - Built-in Parallel Orchestration
# This script runs on VM boot and self-manages parallel epic execution

set -e

USER="malhitticrypto"
REPO_DIR="/home/$USER/universal-or-strategy"

echo "=========================================="
echo "Golden Image v3 Setup - Parallel Orchestrator"
echo "=========================================="
echo "Start: $(date)"

# Update system
apt-get update
apt-get upgrade -y

# Install required packages
apt-get install -y \
    git \
    curl \
    wget \
    build-essential \
    python3 \
    python3-pip \
    screen \
    jq

# Install mise (modern runtime manager)
su - $USER -c "curl https://mise.run | sh"
su - $USER -c "echo 'eval \"\$(~/.local/bin/mise activate bash)\"' >> ~/.bashrc"

# Configure npm user-level prefix
su - $USER -c "npm config set prefix ~/.npm-global"
su - $USER -c "echo 'export PATH=~/.npm-global/bin:\$PATH' >> ~/.bashrc"

# Install Bob Shell via official installer
su - $USER -c "bash -l -c 'curl -fsSL https://bob.ibm.com/download/bobshell.sh | bash'"

# Configure global git identity
su - $USER -c "git config --global user.email 'malhitticrypto@gmail.com'"
su - $USER -c "git config --global user.name 'malhitticrypto'"

# Set Bob Shell API key
BOB_API_KEY="bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5"
su - $USER -c "echo 'export BOBSHELL_API_KEY=$BOB_API_KEY' >> ~/.bashrc"

# Create orchestration script
cat > /home/$USER/run_parallel_epics.sh << 'ORCHESTRATOR_EOF'
#!/bin/bash
# Parallel Epic Orchestrator
# Reads epic list from VM metadata and executes in parallel using screen

set -e

echo "=========================================="
echo "Parallel Epic Orchestrator"
echo "=========================================="
echo "Start: $(date)"

# Load environment
source ~/.bashrc

# Get epic list from VM metadata
EPIC_JSON=$(curl -s "http://metadata.google.internal/computeMetadata/v1/instance/attributes/epics" -H "Metadata-Flavor: Google")

if [ -z "$EPIC_JSON" ] || [ "$EPIC_JSON" == "" ]; then
    echo "ERROR: No epic list found in VM metadata"
    echo "Expected metadata key: epics"
    echo "Expected format: JSON array of {id, method, cyc}"
    exit 1
fi

echo "Epic list received:"
echo "$EPIC_JSON" | jq .

# Parse epic count
EPIC_COUNT=$(echo "$EPIC_JSON" | jq 'length')
echo ""
echo "Total epics to process: $EPIC_COUNT"
echo ""

# Create logs directory
cd ~/universal-or-strategy
mkdir -p logs

# Launch each epic in a screen session
echo "Launching $EPIC_COUNT parallel agents..."
echo ""

for i in $(seq 0 $(($EPIC_COUNT - 1))); do
    EPIC_ID=$(echo "$EPIC_JSON" | jq -r ".[$i].id")
    METHOD=$(echo "$EPIC_JSON" | jq -r ".[$i].method")
    CYC=$(echo "$EPIC_JSON" | jq -r ".[$i].cyc")
    
    echo "[$((i+1))/$EPIC_COUNT] Launching $EPIC_ID ($METHOD, CYC $CYC)"
    
    # Create screen session with proper quoting
    screen -dmS "$EPIC_ID" bash -c "
        cd ~/universal-or-strategy
        source ~/.bashrc
        bob --accept-license --auth-method api-key -p \"Run epic-intake for $EPIC_ID. Target: Reduce complexity in $METHOD (CYC $CYC to 8)\" --max-coins 30 > logs/${EPIC_ID}.log 2>&1
        echo \"$EPIC_ID complete\" >> logs/${EPIC_ID}.log
    "
    
    echo "  ✓ Screen session '$EPIC_ID' started"
    sleep 1
done

echo ""
echo "All $EPIC_COUNT agents launched!"
echo ""

# Monitor progress
echo "Monitoring progress (checking every 60 seconds)..."
echo ""

while true; do
    ACTIVE=$(screen -ls | grep -c "EPIC-CCN" || true)
    
    if [ $ACTIVE -eq 0 ]; then
        echo "$(date): All agents completed!"
        break
    fi
    
    echo "$(date): $ACTIVE / $EPIC_COUNT agents still running"
    
    # Show log sizes
    for i in $(seq 0 $(($EPIC_COUNT - 1))); do
        EPIC_ID=$(echo "$EPIC_JSON" | jq -r ".[$i].id")
        if [ -f "logs/${EPIC_ID}.log" ]; then
            LINES=$(wc -l < "logs/${EPIC_ID}.log")
            echo "  $EPIC_ID: $LINES lines"
        fi
    done
    
    echo ""
    sleep 60
done

echo ""
echo "=========================================="
echo "Orchestration Complete"
echo "=========================================="
echo "End: $(date)"
echo ""
echo "Logs available in: ~/universal-or-strategy/logs/"
echo "Artifacts available in: ~/universal-or-strategy/docs/brain/"

ORCHESTRATOR_EOF

chmod +x /home/$USER/run_parallel_epics.sh
chown $USER:$USER /home/$USER/run_parallel_epics.sh

echo "✓ Orchestration script created"

# Create systemd service to run orchestrator on boot
cat > /etc/systemd/system/epic-orchestrator.service << 'SERVICE_EOF'
[Unit]
Description=Epic Parallel Orchestrator
After=network.target google-network-daemon.service

[Service]
Type=oneshot
User=malhitticrypto
WorkingDirectory=/home/malhitticrypto
ExecStart=/home/malhitticrypto/run_parallel_epics.sh
StandardOutput=journal
StandardError=journal
RemainAfterExit=yes

[Install]
WantedBy=multi-user.target
SERVICE_EOF

systemctl daemon-reload
systemctl enable epic-orchestrator.service

echo "✓ Systemd service configured"

echo ""
echo "=========================================="
echo "Golden Image v3 Setup Complete"
echo "=========================================="
echo "End: $(date)"
echo ""
echo "Features:"
echo "- Bob Shell with API key authentication"
echo "- mise runtime manager"
echo "- screen for persistent sessions"
echo "- Built-in parallel orchestrator"
echo "- Systemd service for auto-start"
echo ""
echo "Usage:"
echo "1. Create VM from this image"
echo "2. Pass epic list via metadata key 'epics'"
echo "3. Orchestrator runs automatically on boot"
echo "4. Monitor via: gcloud compute ssh VM --command='screen -ls'"
echo "5. View logs via: gcloud compute ssh VM --command='tail -f ~/universal-or-strategy/logs/*.log'"

# Made with Bob
