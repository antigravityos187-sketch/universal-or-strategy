#!/bin/bash
set -e

echo "=== VM Setup and Test Execution ==="

# Fix DNS
echo "Checking DNS..."
if ! grep -q "8.8.8.8" /etc/resolv.conf 2>/dev/null; then
    echo "nameserver 8.8.8.8" | sudo tee -a /etc/resolv.conf > /dev/null
    echo "nameserver 8.8.4.4" | sudo tee -a /etc/resolv.conf > /dev/null
    echo "DNS fixed"
fi

# Clone or update repository
cd /home/malhitticrypto
if [ ! -d "universal-or-strategy" ]; then
    echo "Cloning repository..."
    git clone https://github.com/malhitticrypto-debug/universal-or-strategy.git
else
    echo "Repository exists, updating..."
    cd universal-or-strategy
    git fetch origin main
    git merge origin/main
    cd ..
fi

# Install Bob Shell (try curl first, then wget)
if ! command -v bob &> /dev/null; then
    echo "Installing Bob Shell..."
    if curl -fsSL https://bob.build/install.sh | bash 2>/dev/null; then
        echo "Bob Shell installed via curl"
    elif wget -qO- https://bob.build/install.sh | bash 2>/dev/null; then
        echo "Bob Shell installed via wget"
    else
        echo "WARNING: Bob Shell not found, but continuing..."
    fi
fi

# Show Bob version if available
if command -v bob &> /dev/null; then
    echo "Bob version: $(bob --version)"
fi

# Export API keys
export BOB_API_KEY_1="bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5"
export BOB_API_KEY_2="bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp"
echo "API keys exported"

# Start test execution in tmux
cd /home/malhitticrypto/universal-or-strategy

# Check if wave2_simple_orchestrator.py exists
if [ ! -f "scripts/wave2_simple_orchestrator.py" ]; then
    echo "ERROR: wave2_simple_orchestrator.py not found at scripts/wave2_simple_orchestrator.py"
    echo "Directory contents:"
    ls -lah scripts/ | head -20
    exit 1
fi

# Create test config
cat > /tmp/test_config.json << 'EOF'
{
  "epics": [
    {
      "epic_id": "EPIC-CCN-164",
      "method": "IsCommandForThisInstrument",
      "file": "src/V12_002.UI.IPC.cs",
      "cyc": 36,
      "api_key": "bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5"
    },
    {
      "epic_id": "EPIC-CCN-107",
      "method": "HydrateFromOpenPositions",
      "file": "src/V12_002.SIMA.Lifecycle.cs",
      "cyc": 31,
      "api_key": "bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp"
    }
  ]
}
EOF

echo "Starting test execution in tmux..."
tmux new-session -d -s epic-test "python3 scripts/wave2_simple_orchestrator.py /tmp/test_config.json"

# Verify tmux session started
if tmux has-session -t epic-test 2>/dev/null; then
    echo "✅ Test started in tmux session 'epic-test'"
    tmux list-sessions
else
    echo "❌ Failed to start tmux session"
    exit 1
fi

# Made with Bob
