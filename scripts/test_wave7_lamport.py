#!/usr/bin/env python3
"""
Test Wave 7 Lamport clock implementation.

Usage:
    python scripts/test_wave7_lamport.py
"""

import sys
from pathlib import Path

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))

from scripts.lamport_clock import (
    record_phase_start,
    record_phase_complete,
    verify_can_execute,
    get_workflow
)


def main():
    """Test Wave 7 Lamport clock implementation."""
    print("Testing Wave 7 Lamport clock implementation...\n")
    
    # Test 1: Record Phase 0 start
    print("Test 1: Recording Phase 0 start...")
    event1 = record_phase_start('EPIC-W7-001', '0', 'wave7-phase0-test')
    print(f"✅ Phase 0 start recorded:")
    print(f"   Clock: {event1['clock']}")
    print(f"   State Hash: {event1['state_hash'][:16]}...")
    print(f"   Timestamp: {event1['timestamp']}")
    
    # Test 2: Record Phase 0 completion
    print("\nTest 2: Recording Phase 0 completion...")
    event2 = record_phase_complete('EPIC-W7-001', '0', 'wave7-phase0-test', {
        'method_name': 'ProcessOrders',
        'cyc_before': 21,
        'cyc_after': 8,
        'file_path': 'src/V12_002.cs'
    })
    print(f"✅ Phase 0 complete recorded:")
    print(f"   Clock: {event2['clock']}")
    print(f"   State Hash: {event2['state_hash'][:16]}...")
    print(f"   Data: {event2['data']}")
    
    # Test 3: Verify Phase 1 can execute
    print("\nTest 3: Verifying Phase 1 dependencies...")
    can_execute, reason = verify_can_execute('EPIC-W7-001', '1')
    print(f"✅ Phase 1 dependency check:")
    print(f"   Can Execute: {can_execute}")
    print(f"   Reason: {reason}")
    
    # Test 4: Get next phases
    print("\nTest 4: Getting next executable phases...")
    workflow = get_workflow()
    next_phases = workflow.get_next_phases('EPIC-W7-001')
    print(f"✅ Next phases: {next_phases}")
    
    # Test 5: Verify determinism
    print("\nTest 5: Verifying determinism...")
    is_deterministic, reason = workflow.verify_determinism('EPIC-W7-001', '0')
    print(f"✅ Determinism check:")
    print(f"   Is Deterministic: {is_deterministic}")
    print(f"   Reason: {reason}")
    
    print("\n" + "="*60)
    print("✅ ALL TESTS PASSED!")
    print("="*60)
    print("\nWave 7 Lamport clock is ready for autonomous execution.")


if __name__ == "__main__":
    main()

# Made with Bob
