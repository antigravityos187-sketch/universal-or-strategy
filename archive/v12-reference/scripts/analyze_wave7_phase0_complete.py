#!/usr/bin/env python3
"""
Comprehensive Wave 7 Phase 0 analysis:
1. Check which epics have exactly 2 files (manifest.json + 00-hotspots.md)
2. Match against Wave 7 methods
3. Check execution timestamps
4. Check if launcher script is still running
"""

import os
import json
import glob
import subprocess
from datetime import datetime

def check_process_running():
    """Check if Python launcher is still running."""
    try:
        result = subprocess.run(
            ['/usr/bin/ps', 'aux'],
            capture_output=True,
            text=True,
            timeout=5
        )
        
        for line in result.stdout.split('\n'):
            if 'launch_wave7_python.py' in line and 'grep' not in line:
                return True, line.strip()
        return False, None
    except Exception as e:
        return False, f"Error checking: {e}"

def get_epic_files(epic_dir):
    """Get list of files in epic directory."""
    try:
        files = os.listdir(epic_dir)
        return sorted([f for f in files if not f.startswith('.')])
    except:
        return []

def get_manifest_timestamp(epic_dir):
    """Extract timestamp from manifest.json."""
    manifest_path = os.path.join(epic_dir, 'manifest.json')
    try:
        with open(manifest_path, 'r') as f:
            data = json.load(f)
            phase_0 = data.get('phases', {}).get('phase_0', {})
            timestamp = phase_0.get('timestamp', '')
            return timestamp
    except:
        return None

def extract_method_from_hotspots(epic_dir):
    """Extract method name from 00-hotspots.md."""
    hotspots_file = os.path.join(epic_dir, "00-hotspots.md")
    if not os.path.exists(hotspots_file):
        return None
    
    try:
        with open(hotspots_file, 'r', encoding='utf-8') as f:
            content = f.read()
            import re
            match = re.search(r'\*\*Method\*\*:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                match = re.search(r'Method:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                match = re.search(r'`([A-Z][a-zA-Z0-9_]+)`', content[:500])
            
            if match:
                return match.group(1)
    except:
        pass
    
    return None

def main():
    print("=" * 80)
    print("WAVE 7 PHASE 0 COMPREHENSIVE ANALYSIS")
    print("=" * 80)
    
    # Check if launcher is running
    print("\n1. CHECKING LAUNCHER PROCESS STATUS")
    print("-" * 80)
    is_running, process_info = check_process_running()
    if is_running:
        print(f"⚠️  Launcher IS RUNNING:")
        print(f"   {process_info}")
    else:
        print(f"✅ Launcher is NOT running")
        if process_info:
            print(f"   Note: {process_info}")
    
    # Load Wave 7 roadmap
    print("\n2. LOADING WAVE 7 ROADMAP")
    print("-" * 80)
    with open('epic_roadmap_wave7.json', 'r') as f:
        roadmap = json.load(f)
    
    wave7_methods = {}
    for epic_id, epic_data in roadmap['epics'].items():
        method_name = epic_data.get('method', '')
        if method_name:
            wave7_methods[method_name] = epic_id
    
    print(f"Wave 7 contains {len(wave7_methods)} methods")
    
    # Find all epic directories
    print("\n3. ANALYZING EPIC DIRECTORIES")
    print("-" * 80)
    all_dirs = []
    all_dirs.extend(glob.glob("docs/brain/EPIC-[0-9]*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-CCN-*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-W7-*"))
    
    print(f"Found {len(all_dirs)} total epic directories")
    
    # Analyze each directory
    phase0_complete = []
    phase0_wave7 = []
    phase0_other = []
    
    for epic_dir in sorted(all_dirs):
        files = get_epic_files(epic_dir)
        
        # Check if Phase 0 complete (exactly 2 files: manifest.json + 00-hotspots.md)
        if len(files) == 2 and 'manifest.json' in files and '00-hotspots.md' in files:
            epic_name = os.path.basename(epic_dir)
            timestamp = get_manifest_timestamp(epic_dir)
            method = extract_method_from_hotspots(epic_dir)
            
            phase0_complete.append({
                'dir': epic_name,
                'method': method,
                'timestamp': timestamp,
                'is_wave7': method in wave7_methods if method else False,
                'correct_id': wave7_methods.get(method, '') if method else ''
            })
            
            if method and method in wave7_methods:
                phase0_wave7.append((epic_name, method, wave7_methods[method], timestamp))
            else:
                phase0_other.append((epic_name, method, timestamp))
    
    # Report results
    print("\n" + "=" * 80)
    print("RESULTS")
    print("=" * 80)
    
    print(f"\n✅ PHASE 0 COMPLETE (2 files): {len(phase0_complete)} epics")
    print(f"   - Wave 7: {len(phase0_wave7)}")
    print(f"   - Other waves: {len(phase0_other)}")
    
    print(f"\n📊 WAVE 7 PHASE 0 COMPLETE ({len(phase0_wave7)} epics):")
    print("-" * 80)
    if phase0_wave7:
        print(f"{'Current Name':<25} {'Method':<40} {'Should Be':<20} {'Timestamp'}")
        print("-" * 80)
        for dir_name, method, correct_id, timestamp in sorted(phase0_wave7, key=lambda x: x[2]):
            ts_short = timestamp[:19] if timestamp else 'N/A'
            needs_rename = "✓" if dir_name == correct_id else "⚠️ RENAME"
            print(f"{dir_name:<25} {method:<40} {correct_id:<20} {ts_short} {needs_rename}")
    
    print(f"\n⚠️  OTHER WAVE PHASE 0 COMPLETE ({len(phase0_other)} epics):")
    if phase0_other:
        for dir_name, method, timestamp in phase0_other[:5]:
            ts_short = timestamp[:19] if timestamp else 'N/A'
            method_str = method if method else 'Unknown'
            print(f"  {dir_name:<25} {method_str:<40} {ts_short}")
        if len(phase0_other) > 5:
            print(f"  ... and {len(phase0_other) - 5} more")
    
    # Calculate remaining
    remaining = 161 - len(phase0_wave7)
    
    print("\n" + "=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"Wave 7 Phase 0 Complete: {len(phase0_wave7)}/161 ({len(phase0_wave7)*100//161}%)")
    print(f"Wave 7 Phase 0 Remaining: {remaining}/161 ({remaining*100//161}%)")
    print(f"Other waves Phase 0 complete: {len(phase0_other)}")
    print(f"Launcher running: {'YES ⚠️' if is_running else 'NO ✅'}")
    print("=" * 80)
    
    # Save results
    with open('wave7_phase0_status.json', 'w') as f:
        json.dump({
            'timestamp': datetime.now().isoformat(),
            'launcher_running': is_running,
            'wave7_complete': len(phase0_wave7),
            'wave7_remaining': remaining,
            'other_waves_complete': len(phase0_other),
            'wave7_epics': phase0_wave7
        }, f, indent=2)
    
    print("\n✅ Results saved to wave7_phase0_status.json")

if __name__ == "__main__":
    main()

# Made with Bob
