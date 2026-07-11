#!/usr/bin/env python3
"""Analyze Wave 4 commit to determine PR cluster distribution."""

import subprocess
import json
from pathlib import Path
from collections import defaultdict

# Architecture mapping from PR_REVIEW_CLUSTER_STRATEGY.md
SUBSYSTEM_MAPPING = {
    "S1: SIMA Core": [
        "V12_002.SIMA.cs",
        "V12_002.SIMA.Dispatch.cs",
        "V12_002.SIMA.Fleet.cs",
        "V12_002.SIMA.Flatten.cs",
        "V12_002.SIMA.Lifecycle.cs",
        "V12_002.SIMA.Shadow.cs"
    ],
    "S2: Execution Engine": [
        "V12_002.Orders.Callbacks.cs",
        "V12_002.Orders.Callbacks.Execution.cs",
        "V12_002.Orders.Callbacks.Propagation.cs",
        "V12_002.Orders.Management.Cleanup.cs",
        "V12_002.Orders.Management.Flatten.cs",
        "V12_002.Orders.Management.StopSync.cs",
        "V12_002.Symmetry.BracketFSM.cs"
    ],
    "S3: UI & Photon IO": [
        "V12_002.UI.IPC.cs",
        "V12_002.UI.IPC.Commands.Config.cs",
        "V12_002.UI.IPC.Commands.Fleet.cs",
        "V12_002.UI.Panel.Handlers.cs",
        "V12_002.UI.Panel.Helpers.cs",
        "V12_002.IPC.Hardening.cs"
    ],
    "S4: REAPER Defense": [
        "V12_002.REAPER.NakedPosition.cs",
        "V12_002.REAPER.OrphanSafety.cs"
    ],
    "S5: Kernel State": [
        "V12_002.cs",
        "V12_002.Lifecycle.cs",
        "V12_002.PositionInfo.cs"
    ],
    "S6: Signals & Entries": [
        "V12_002.Entries.FFMA.cs",
        "V12_002.Entries.RMA.cs",
        "V12_002.BarUpdate.cs",
        "V12_002.Trailing.StopUpdate.cs"
    ],
    "S7: Kernel Infrastructure": [
        "V12_002.Telemetry.cs"
    ]
}

def get_commit_stats(commit_sha):
    """Get file-level stats for a commit."""
    result = subprocess.run(
        ["git", "show", "--numstat", commit_sha],
        capture_output=True,
        text=True,
        check=True
    )
    
    file_stats = {}
    for line in result.stdout.split('\n'):
        parts = line.split('\t')
        if len(parts) == 3:
            added, deleted, filepath = parts
            if filepath.startswith('src/') and filepath.endswith('.cs'):
                filename = Path(filepath).name
                try:
                    file_stats[filename] = {
                        'added': int(added),
                        'deleted': int(deleted),
                        'net': int(added) - int(deleted),
                        'total': int(added) + int(deleted)
                    }
                except ValueError:
                    # Binary files show '-' instead of numbers
                    pass
    
    return file_stats

def map_files_to_subsystems(file_stats):
    """Map files to subsystems and calculate cluster stats."""
    clusters = defaultdict(lambda: {
        'files': [],
        'added': 0,
        'deleted': 0,
        'net': 0,
        'total': 0
    })
    
    unmapped = []
    
    for filename, stats in file_stats.items():
        mapped = False
        for subsystem, files in SUBSYSTEM_MAPPING.items():
            if filename in files:
                clusters[subsystem]['files'].append(filename)
                clusters[subsystem]['added'] += stats['added']
                clusters[subsystem]['deleted'] += stats['deleted']
                clusters[subsystem]['net'] += stats['net']
                clusters[subsystem]['total'] += stats['total']
                mapped = True
                break
        
        if not mapped:
            unmapped.append(filename)
    
    return dict(clusters), unmapped

def main():
    commit_sha = "253305dc"
    
    print(f"Analyzing Wave 4 commit: {commit_sha}\n")
    
    # Get file stats
    file_stats = get_commit_stats(commit_sha)
    
    # Map to subsystems
    clusters, unmapped = map_files_to_subsystems(file_stats)
    
    # Print results
    print("=" * 80)
    print("PR CLUSTER ANALYSIS")
    print("=" * 80)
    
    pr_num = 1
    total_files = 0
    total_added = 0
    total_deleted = 0
    total_net = 0
    total_changes = 0
    
    for subsystem in ["S1: SIMA Core", "S2: Execution Engine", "S3: UI & Photon IO", 
                      "S4: REAPER Defense", "S5: Kernel State", "S6: Signals & Entries", 
                      "S7: Kernel Infrastructure"]:
        if subsystem in clusters:
            cluster = clusters[subsystem]
            print(f"\nPR-{pr_num}: {subsystem}")
            print(f"  Files: {len(cluster['files'])}")
            print(f"  Lines Added: {cluster['added']:,}")
            print(f"  Lines Deleted: {cluster['deleted']:,}")
            print(f"  Net Change: {cluster['net']:+,}")
            print(f"  Total Changes: {cluster['total']:,}")
            print(f"  Files:")
            for f in sorted(cluster['files']):
                stats = file_stats[f]
                print(f"    - {f}: +{stats['added']} -{stats['deleted']} (net: {stats['net']:+})")
            
            total_files += len(cluster['files'])
            total_added += cluster['added']
            total_deleted += cluster['deleted']
            total_net += cluster['net']
            total_changes += cluster['total']
            pr_num += 1
    
    print("\n" + "=" * 80)
    print("TOTALS")
    print("=" * 80)
    print(f"Total PRs: {pr_num - 1}")
    print(f"Total Files: {total_files}")
    print(f"Total Lines Added: {total_added:,}")
    print(f"Total Lines Deleted: {total_deleted:,}")
    print(f"Net Change: {total_net:+,}")
    print(f"Total Changes: {total_changes:,}")
    
    if unmapped:
        print("\n" + "=" * 80)
        print("UNMAPPED FILES (not in any subsystem)")
        print("=" * 80)
        for f in sorted(unmapped):
            stats = file_stats[f]
            print(f"  - {f}: +{stats['added']} -{stats['deleted']} (net: {stats['net']:+})")
    
    # Calculate diff size estimate
    print("\n" + "=" * 80)
    print("DIFF SIZE ESTIMATES")
    print("=" * 80)
    print(f"Total diff lines (added + deleted): {total_changes:,}")
    print(f"Average per PR: {total_changes // (pr_num - 1):,}")
    print(f"\nPR Hygiene Check:")
    if total_changes < 10000:
        print(f"  ✅ PASS - Total changes ({total_changes:,}) < 10,000 threshold")
    else:
        print(f"  ⚠️  WARNING - Total changes ({total_changes:,}) exceeds 10,000 threshold")
        print(f"  Consider splitting into smaller PRs")

if __name__ == "__main__":
    main()

# Made with Bob
