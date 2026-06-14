#!/bin/bash
# Launch Phase 0 for EPIC-006 through EPIC-015 in tmux split panes
# Creates 2x5 grid (10 panes) - watch all epics simultaneously

set -e

echo "=== Wave 1 Phase 0 Tmux Launcher ==="
echo "Creating 2x5 grid for 10 epics..."

# Check if tmux is installed
if ! command -v tmux &> /dev/null; then
    echo "ERROR: tmux not installed"
    echo "Install with: sudo apt-get install tmux"
    exit 1
fi

# Kill existing session if it exists
tmux kill-session -t wave1-p0 2>/dev/null || true

# Create new session with first epic
tmux new-session -d -s wave1-p0 -n "Phase0" "cd /home/malhitticrypto/universal-or-strategy && bash _p0_006.sh 2>&1 | tee logs/phase0/EPIC-006.log; echo 'EPIC-006 DONE'; read"

# Split into 2 columns
tmux split-window -h -t wave1-p0:0.0 "cd /home/malhitticrypto/universal-or-strategy && bash _p0_007.sh 2>&1 | tee logs/phase0/EPIC-007.log; echo 'EPIC-007 DONE'; read"

# Split left column into 5 rows (006, 008, 010, 012, 014)
tmux select-pane -t wave1-p0:0.0
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_008.sh 2>&1 | tee logs/phase0/EPIC-008.log; echo 'EPIC-008 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_010.sh 2>&1 | tee logs/phase0/EPIC-010.log; echo 'EPIC-010 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_012.sh 2>&1 | tee logs/phase0/EPIC-012.log; echo 'EPIC-012 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_014.sh 2>&1 | tee logs/phase0/EPIC-014.log; echo 'EPIC-014 DONE'; read"

# Split right column into 5 rows (007, 009, 011, 013, 015)
tmux select-pane -t wave1-p0:0.1
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_009.sh 2>&1 | tee logs/phase0/EPIC-009.log; echo 'EPIC-009 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_011.sh 2>&1 | tee logs/phase0/EPIC-011.log; echo 'EPIC-011 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_013.sh 2>&1 | tee logs/phase0/EPIC-013.log; echo 'EPIC-013 DONE'; read"
tmux split-window -v "cd /home/malhitticrypto/universal-or-strategy && bash _p0_015.sh 2>&1 | tee logs/phase0/EPIC-015.log; echo 'EPIC-015 DONE'; read"

# Balance panes for equal sizing
tmux select-layout -t wave1-p0:0 tiled

# Enable mouse support for scrolling
tmux set-option -t wave1-p0 mouse on

# Enable pane synchronization (optional - sends same input to all panes)
# Uncomment if you want to control all panes together
# tmux set-window-option -t wave1-p0:0 synchronize-panes on

echo ""
echo "=== Tmux Session Created ==="
echo "Session name: wave1-p0"
echo "Layout: 2x5 grid (10 panes)"
echo ""
echo "To attach and watch:"
echo "  tmux attach -t wave1-p0"
echo ""
echo "Tmux controls:"
echo "  Ctrl+B then Arrow Keys - Navigate between panes"
echo "  Ctrl+B then Z - Zoom current pane (toggle fullscreen)"
echo "  Ctrl+B then [ - Enter scroll mode (q to exit)"
echo "  Ctrl+B then D - Detach (keeps running)"
echo "  Mouse scroll - Scroll in any pane"
echo ""
echo "To check status from outside:"
echo "  tmux list-panes -t wave1-p0 -F '#{pane_index}: #{pane_current_command}'"
echo ""
echo "To kill session:"
echo "  tmux kill-session -t wave1-p0"
echo ""
echo "Attaching now..."
sleep 2

# Attach to session
tmux attach -t wave1-p0

# Made with Bob
