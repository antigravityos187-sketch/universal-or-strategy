#!/usr/bin/env python3
"""Verify Phase 1.5 can execute."""
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import verify_can_execute

can_execute, reason = verify_can_execute('EPIC-CCN-001', '1.5', 'alprofit')
print(f'Can execute: {can_execute}')
print(f'Reason: {reason}')
sys.exit(0 if can_execute else 1)

# Made with Bob
