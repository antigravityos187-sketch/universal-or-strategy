#!/usr/bin/env python3
"""
Lamport Clock Cleanup for EPIC-CCN-003 Phase 1
Adds phase_fail event to close out stale phase_start (clock 177)
"""

import json
import os
from datetime import datetime

LAMPORT_LOG = ".lamport/event_log.jsonl"

def append_phase_fail():
    """Append phase_fail event to close out clock 177"""
    
    # Read current max clock
    max_clock = 0
    if os.path.exists(LAMPORT_LOG):
        with open(LAMPORT_LOG, 'r') as f:
            for line in f:
                if line.strip():
                    event = json.loads(line)
                    max_clock = max(max_clock, event.get('clock', 0))
    
    # Create phase_fail event
    fail_event = {
        "clock": max_clock + 1,
        "event_type": "phase_fail",
        "epic_id": "EPIC-CCN-003",
        "phase": "1",
        "agent_id": "wave6-p1-003",
        "status": "failed",
        "state_hash": "f17da40ccefda979d85355ed4cb677a3dccd0af495d4b501d474f8a23af2dec8",
        "data": {"error": "Stale session cleanup - frozen session recovery"},
        "timestamp": datetime.utcnow().isoformat()
    }
    
    # Append to log
    with open(LAMPORT_LOG, 'a') as f:
        f.write(json.dumps(fail_event) + '\n')
    
    print(f"✅ Added phase_fail event (clock {fail_event['clock']}) to close out stale Phase 1 execution")
    print(f"   Epic: EPIC-CCN-003, Phase: 1, Agent: wave6-p1-003")
    print(f"   Reason: Frozen session recovery")

if __name__ == "__main__":
    append_phase_fail()

# Made with Bob
