#!/bin/bash
set -e

echo "=== VM Setup and Test Execution ==="

# Navigate to repo
cd ~/universal-or-strategy || { echo "Repo not found, cloning..."; git clone https://github.com/antigravityos187-sketch/universal-or-strategy.git ~/universal-or-strategy; cd ~/universal-or-strategy; }

# Update repo
echo "Updating repository..."
git pull origin main || echo "Pull failed, continuing..."

# Check if Bob is installed
if ! command -v bob &> /dev/null; then
    echo "Installing Bob Shell..."
    curl -fsSL https://bob.build/install.sh | sh || {
        echo "Bob install failed, trying alternative method..."
        wget -O- https://bob.build/install.sh | sh || {
            echo "ERROR: Cannot install Bob Shell"
            exit 1
        }
    }
    # Add to PATH
    export PATH="$HOME/.bob/bin:$PATH"
fi

echo "Bob version:"
bob --version || echo "Bob not found in PATH"

# Export API keys
export API_KEYS="bob_prod_bob-admin_4UXUt9vwr3DKi2jrP1dEiXvaFmhdsqerpRo1bkVFZYLtod9BWoa82vRKNW2JvLNFiMCXiKWhAyhdHYjgsxCNoMDF_HzsjucwNDH4LGfvXN21q8jECWiaErhvvr9z9h474jEp5,bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp"

echo "API keys exported"

# Check if autonomous_executor.py exists
if [ ! -f "scripts/autonomous_executor.py" ]; then
    echo "ERROR: autonomous_executor.py not found"
    exit 1
fi

echo "Starting test execution in tmux..."
# Start tmux session and run test
tmux new-session -d -s epic-test "python3 scripts/autonomous_executor.py --api-keys \"$API_KEYS\" --workers 2 --epics EPIC-CCN-164,EPIC-CCN-107"

echo "✅ Test started in tmux session 'epic-test'"
echo "To attach: tmux attach -t epic-test"
echo "To detach: Ctrl+B, then D"

# Made with Bob
