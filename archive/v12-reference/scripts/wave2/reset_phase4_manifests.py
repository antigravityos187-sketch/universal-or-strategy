#!/usr/bin/env python3
"""Reset Phase 4 status in manifests to 'pending' so agents can run."""

import json
from pathlib import Path

EPIC_IDS = [
    "107", "108", "109", "110", "111", "112", "113", "114", "115"
]

REPO_ROOT = Path("c:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_ROOT / "docs/brain"

def reset_phase4(epic_id: str):
    """Reset Phase 4 status to pending"""
    manifest_path = BRAIN_DIR / f"EPIC-CCN-{epic_id}" / "manifest.json"
    
    if not manifest_path.exists():
        print(f"[SKIP] EPIC-CCN-{epic_id}: No manifest")
        return
    
    manifest = json.loads(manifest_path.read_text())
    
    # Reset the numeric "4" key (used by launch script)
    if "4" in manifest["phases"]:
        manifest["phases"]["4"]["status"] = "pending"
        print(f"[RESET] EPIC-CCN-{epic_id}: Phase 4 -> pending")
    else:
        # Add if missing
        manifest["phases"]["4"] = {
            "status": "pending",
            "output": "04-tickets.md"
        }
        print(f"[ADD] EPIC-CCN-{epic_id}: Added Phase 4 entry")
    
    manifest["last_updated"] = "2026-06-12T20:27:00Z"
    manifest_path.write_text(json.dumps(manifest, indent=2))

def main():
    print("[RESET] Resetting Phase 4 manifests for Wave 2 epics")
    print("=" * 60)
    
    for epic_id in EPIC_IDS:
        reset_phase4(epic_id)
    
    print("\n[DONE] All manifests reset to pending")
    print("[NEXT] Re-run: python scripts/wave2/phase4_with_checkpoints.py")

if __name__ == "__main__":
    main()

# Made with Bob
