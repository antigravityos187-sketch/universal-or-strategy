#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Test V12.52 Lamport Causal Verification Implementation

Tests:
1. Lamport clock monotonicity
2. Event log ordering
3. Dependency checking
4. State hash computation
5. Deterministic workflow
6. Manifest integration
"""

import sys
import os
import json
import shutil
from pathlib import Path

# Disable file locking for tests (Windows has issues with msvcrt.locking)
os.environ['EPIC_MANIFEST_NO_LOCK'] = '1'

# Force UTF-8 output on Windows
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# Add scripts directory to path
sys.path.insert(0, str(Path(__file__).parent))

from lamport_clock import (
    DeterministicWorkflow,
    get_workflow,
    record_phase_start,
    record_phase_complete,
    record_phase_fail,
    verify_can_execute
)

from epic_manifest import (
    generate_manifest,
    load_manifest,
    update_manifest,
    validate_dependencies,
    start_phase_execution,
    complete_phase_execution,
    fail_phase_execution,
    get_event_log,
    verify_filesystem_state
)


def cleanup_test_data():
    """Clean up test data from previous runs."""
    # Remove test epic directory
    test_epic_dir = Path("docs/brain/EPIC-TEST-001")
    if test_epic_dir.exists():
        shutil.rmtree(test_epic_dir)
    
    # Remove Lamport workflow directory
    lamport_dir = Path(".lamport")
    if lamport_dir.exists():
        shutil.rmtree(lamport_dir)
    
    print("✓ Cleaned up test data")


def test_lamport_clock_monotonicity():
    """Test 1: Lamport clock increments monotonically."""
    print("\n=== Test 1: Lamport Clock Monotonicity ===")
    
    workflow = get_workflow()
    
    # Record 5 events
    clocks = []
    for i in range(5):
        event = workflow.record_event(
            'test_event',
            'EPIC-TEST-001',
            '0',
            f'test-agent-{i}',
            'running'
        )
        clocks.append(event['clock'])
        print(f"Event {i+1}: clock={event['clock']}")
    
    # Verify monotonicity
    for i in range(len(clocks) - 1):
        assert clocks[i] < clocks[i+1], f"Clock not monotonic: {clocks[i]} >= {clocks[i+1]}"
    
    print("✓ Lamport clock is monotonic")
    return True


def test_event_log_ordering():
    """Test 2: Event log maintains causal order."""
    print("\n=== Test 2: Event Log Ordering ===")
    
    workflow = get_workflow()
    
    # Get event log
    events = workflow.get_event_log('EPIC-TEST-001')
    
    # Verify ordering
    for i in range(len(events) - 1):
        assert events[i]['clock'] <= events[i+1]['clock'], \
            f"Events not in causal order: {events[i]['clock']} > {events[i+1]['clock']}"
    
    print(f"✓ Event log maintains causal order ({len(events)} events)")
    return True


def test_dependency_checking():
    """Test 3: Dependency checking works correctly."""
    print("\n=== Test 3: Dependency Checking ===")
    
    workflow = get_workflow()
    
    # Test Phase 0 (no dependencies)
    satisfied, reason = workflow.check_dependencies('EPIC-TEST-001', '0')
    assert satisfied, f"Phase 0 should have no dependencies: {reason}"
    print("✓ Phase 0: No dependencies (correct)")
    
    # Test Phase 1 (depends on Phase 0)
    satisfied, reason = workflow.check_dependencies('EPIC-TEST-001', '1')
    assert not satisfied, "Phase 1 should be blocked (Phase 0 not complete)"
    print(f"✓ Phase 1: Blocked on Phase 0 (correct) - {reason}")
    
    # Complete Phase 0
    workflow.record_event('phase_complete', 'EPIC-TEST-001', '0', 'test-agent', 'completed')
    
    # Test Phase 1 again
    satisfied, reason = workflow.check_dependencies('EPIC-TEST-001', '1')
    assert satisfied, f"Phase 1 should be unblocked now: {reason}"
    print("✓ Phase 1: Unblocked after Phase 0 complete (correct)")
    
    return True


def test_state_hash_computation():
    """Test 4: State hash computation is deterministic."""
    print("\n=== Test 4: State Hash Computation ===")
    
    workflow = get_workflow()
    
    # Compute hash twice
    hash1 = workflow._compute_state_hash('EPIC-TEST-001', '0')
    hash2 = workflow._compute_state_hash('EPIC-TEST-001', '0')
    
    assert hash1 == hash2, "State hash should be deterministic"
    print(f"✓ State hash is deterministic: {hash1[:16]}...")
    
    return True


def test_deterministic_workflow():
    """Test 5: Workflow determinism verification."""
    print("\n=== Test 5: Deterministic Workflow ===")
    
    workflow = get_workflow()
    
    # Verify determinism for Phase 0
    is_deterministic, reason = workflow.verify_determinism('EPIC-TEST-001', '0')
    assert is_deterministic, f"Workflow should be deterministic: {reason}"
    print(f"✓ Workflow is deterministic: {reason}")
    
    return True


def test_manifest_integration():
    """Test 6: Manifest integration with V12.52."""
    print("\n=== Test 6: Manifest Integration ===")
    
    # Generate test manifest
    manifest = generate_manifest(
        'EPIC-TEST-001',
        'Test epic for V12.52 verification'
    )
    print(f"✓ Generated manifest: {manifest['_path']}")
    
    # Start Phase 0 execution (BEFORE creating output file)
    started, reason = start_phase_execution('EPIC-TEST-001', '0', 'test-agent')
    assert started, f"Failed to start Phase 0: {reason}"
    print(f"✓ Started Phase 0: {reason}")
    
    # Create test output file (DURING phase execution)
    test_output = Path("docs/brain/EPIC-TEST-001/00-hotspots.md")
    test_output.write_text("# Test Hotspot Analysis\n\nTest content\n")
    print(f"✓ Created test output: {test_output}")
    
    # Verify manifest updated
    manifest = load_manifest('EPIC-TEST-001')
    assert manifest['phases']['0']['status'] == 'in_progress', "Phase 0 should be in_progress"
    print("✓ Manifest updated to in_progress")
    
    # Complete Phase 0 execution (use forward slashes for cross-platform compatibility)
    completed, reason = complete_phase_execution(
        'EPIC-TEST-001',
        '0',
        'test-agent',
        [str(test_output).replace('\\', '/')],
        'Test completion'
    )
    assert completed, f"Failed to complete Phase 0: {reason}"
    print(f"✓ Completed Phase 0: {reason}")
    
    # Verify manifest updated
    manifest = load_manifest('EPIC-TEST-001')
    assert manifest['phases']['0']['status'] == 'completed', "Phase 0 should be completed"
    print("✓ Manifest updated to completed")
    
    # Verify event log
    events = get_event_log('EPIC-TEST-001', '0')
    assert len(events) >= 2, "Should have at least 2 events (start + complete)"
    print(f"✓ Event log has {len(events)} events")
    
    # Verify Phase 1 can now execute (using epic_manifest.verify_can_execute with 3 params)
    from epic_manifest import verify_can_execute as manifest_verify_can_execute
    can_execute, reason = manifest_verify_can_execute('EPIC-TEST-001', '1', 'test-agent')
    assert can_execute, f"Phase 1 should be executable now: {reason}"
    print(f"✓ Phase 1 ready to execute: {reason}")
    
    return True


def test_filesystem_state_verification():
    """Test 7: Filesystem state verification."""
    print("\n=== Test 7: Filesystem State Verification ===")
    
    # Verify Phase 0 state (should be clean after completion)
    state_ok, reason = verify_filesystem_state('EPIC-TEST-001', '0')
    assert state_ok, f"Phase 0 state should be valid: {reason}"
    print(f"✓ Phase 0 state valid: {reason}")
    
    # Verify Phase 1 state (should be clean, no stale files)
    state_ok, reason = verify_filesystem_state('EPIC-TEST-001', '1')
    assert state_ok, f"Phase 1 state should be valid: {reason}"
    print(f"✓ Phase 1 state valid: {reason}")
    
    return True


def test_failure_handling():
    """Test 8: Failure handling and recovery."""
    print("\n=== Test 8: Failure Handling ===")
    
    # Start Phase 1
    started, reason = start_phase_execution('EPIC-TEST-001', '1', 'test-agent')
    assert started, f"Failed to start Phase 1: {reason}"
    print(f"✓ Started Phase 1: {reason}")
    
    # Fail Phase 1
    recorded, reason = fail_phase_execution(
        'EPIC-TEST-001',
        '1',
        'test-agent',
        'Test failure'
    )
    assert recorded, f"Failed to record failure: {reason}"
    print(f"✓ Recorded failure: {reason}")
    
    # Verify manifest updated
    manifest = load_manifest('EPIC-TEST-001')
    assert manifest['phases']['1']['status'] == 'failed', "Phase 1 should be failed"
    print("✓ Manifest updated to failed")
    
    # Verify event log
    events = get_event_log('EPIC-TEST-001', '1')
    assert any(e['event_type'] == 'phase_fail' for e in events), "Should have phase_fail event"
    print("✓ Event log contains phase_fail event")
    
    return True


def run_all_tests():
    """Run all V12.52 tests."""
    print("=" * 60)
    print("V12.52 Lamport Causal Verification Test Suite")
    print("=" * 60)
    
    # Clean up before tests
    cleanup_test_data()
    
    tests = [
        test_lamport_clock_monotonicity,
        test_event_log_ordering,
        test_dependency_checking,
        test_state_hash_computation,
        test_deterministic_workflow,
        test_manifest_integration,
        test_filesystem_state_verification,
        test_failure_handling
    ]
    
    passed = 0
    failed = 0
    
    for test in tests:
        try:
            if test():
                passed += 1
        except AssertionError as e:
            print(f"✗ Test failed: {e}")
            failed += 1
        except Exception as e:
            print(f"✗ Test error: {e}")
            failed += 1
    
    print("\n" + "=" * 60)
    print(f"Test Results: {passed} passed, {failed} failed")
    print("=" * 60)
    
    if failed == 0:
        print("\n✓ ALL TESTS PASSED - V12.52 implementation is working correctly!")
        return 0
    else:
        print(f"\n✗ {failed} TESTS FAILED - Fix issues before deploying to VM")
        return 1


if __name__ == '__main__':
    sys.exit(run_all_tests())

# Made with Bob
