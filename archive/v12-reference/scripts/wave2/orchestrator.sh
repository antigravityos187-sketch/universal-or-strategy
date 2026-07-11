#!/bin/bash
# =============================================================================
# V12 Wave Orchestrator - Runs ON the VM at startup via GCP metadata agent
# Designed for golden image v3+ (BOBSHELL_API_KEY in ~/.profile)
#
# FIX vs previous attempts:
#   - Uses "bash -l -c" in screen sessions (sources ~/.profile, sets PATH)
#   - Reads BOBSHELL_API_KEY from user's ~/.profile (already in golden image v3)
#   - Git global identity set before Bob runs (Bob's checkpointing requires it)
# =============================================================================

set -euo pipefail

LOG="/var/log/v12-orchestrator.log"
exec > >(tee -a "$LOG") 2>&1

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] V12 Orchestrator starting..."

# -- Read config from GCP instance metadata --
METADATA_URL="http://metadata.google.internal/computeMetadata/v1/instance/attributes"
METADATA_HDR="Metadata-Flavor: Google"

REPO_URL=$(curl -sf -H "$METADATA_HDR" "$METADATA_URL/v12-repo-url" 2>/dev/null \
           || echo "https://github.com/malhitticrypto-debug/universal-or-strategy.git")
EPICS_RAW=$(curl -sf -H "$METADATA_HDR" "$METADATA_URL/v12-epics" 2>/dev/null || echo "")
MAX_COINS=$(curl -sf -H "$METADATA_HDR" "$METADATA_URL/v12-max-coins" 2>/dev/null || echo "50")
RUN_USER=$(curl -sf -H "$METADATA_HDR" "$METADATA_URL/v12-run-user" 2>/dev/null \
           || echo "malhitticrypto")

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] user=$RUN_USER  epics=$EPICS_RAW  max-coins=$MAX_COINS"

if [[ -z "$EPICS_RAW" ]]; then
  echo "[FATAL] No epics in metadata key 'v12-epics'. Aborting."
  exit 1
fi

USER_HOME="/home/$RUN_USER"
REPO_DIR="$USER_HOME/universal-or-strategy"

# -- Global git identity (required by Bob's checkpointing) --
sudo -u "$RUN_USER" git config --global user.email "malhitticrypto@gmail.com"
sudo -u "$RUN_USER" git config --global user.name "malhitticrypto"
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Git global identity set."

# -- Clone or update repo --
if [[ -d "$REPO_DIR/.git" ]]; then
  echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Repo exists, pulling latest..."
  sudo -u "$RUN_USER" git -C "$REPO_DIR" pull --ff-only origin main \
    || echo "[WARN] git pull failed, continuing with existing state"
else
  echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Cloning repo..."
  sudo -u "$RUN_USER" git clone "$REPO_URL" "$REPO_DIR"
fi

# -- Create log directory --
sudo -u "$RUN_USER" mkdir -p "$REPO_DIR/logs"

# -- Launch parallel screen sessions (one per epic) --
IFS=',' read -ra EPICS <<< "$EPICS_RAW"
LAUNCHED=0

for EPIC in "${EPICS[@]}"; do
  EPIC=$(echo "$EPIC" | xargs)   # trim whitespace
  [[ -z "$EPIC" ]] && continue

  SESSION="v12-$EPIC"
  LOG_FILE="$REPO_DIR/logs/$EPIC.log"

  echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Launching: $SESSION"

  # KEY FIX: "bash -l" sources ~/.profile which sets PATH and BOBSHELL_API_KEY
  # Without -l, screen inherits root's empty PATH and bob is not found
  sudo -u "$RUN_USER" screen -dmS "$SESSION" \
    bash -l -c "cd $REPO_DIR && bob --accept-license --max-coins $MAX_COINS -p 'Run epic-intake for $EPIC' > $LOG_FILE 2>&1; echo EXIT_CODE=\$? >> $LOG_FILE"

  LAUNCHED=$((LAUNCHED + 1))
  sleep 1   # stagger launches slightly to avoid API hammering
done

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Launched $LAUNCHED screen sessions."
sudo -u "$RUN_USER" screen -ls 2>/dev/null || true

# -- Write status file for Antigravity to poll --
cat > /tmp/v12-orchestrator-status <<EOF
LAUNCHED=$LAUNCHED
TIMESTAMP=$(date -u +%Y-%m-%dT%H:%M:%SZ)
EPICS=$EPICS_RAW
MAX_COINS=$MAX_COINS
EOF

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Orchestrator complete. $LAUNCHED agents running."
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Monitor: sudo -u $RUN_USER screen -ls"
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Logs: $REPO_DIR/logs/"
