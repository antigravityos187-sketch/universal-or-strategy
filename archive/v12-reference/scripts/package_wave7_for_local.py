#!/usr/bin/env python3
"""
Package Wave 7 Phase 0 work for transfer to local machine
Creates a compressed archive that can be downloaded via SCP
"""

import os
import tarfile
import shutil
from datetime import datetime
from pathlib import Path

def main():
    print("=" * 70)
    print("Wave 7 Phase 0 - Package for Local Backup")
    print("=" * 70)
    print()
    
    # Configuration
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    archive_name = f"wave7_phase0_complete_{timestamp}.tar.gz"
    temp_dir = f"/tmp/wave7_backup_{os.getpid()}"
    home_dir = os.path.expanduser("~")
    
    try:
        # Create temporary staging directory
        print("Creating temporary staging directory...")
        os.makedirs(temp_dir, exist_ok=True)
        
        # Step 1: Copy epic brain directories
        print("\nStep 1: Copying Wave 7 epic brain directories (161 epics)...")
        brain_dest = os.path.join(temp_dir, "docs/brain")
        os.makedirs(brain_dest, exist_ok=True)
        
        epic_count = 0
        for i in range(1, 162):
            epic_id = f"EPIC-W7-{i:03d}"
            src = f"docs/brain/{epic_id}"
            if os.path.exists(src):
                shutil.copytree(src, os.path.join(brain_dest, epic_id))
                epic_count += 1
        
        print(f"  ✅ Copied {epic_count} epic directories")
        
        # Step 2: Copy building-blocks/wave7
        print("\nStep 2: Copying building-blocks/wave7/...")
        bb_src = "building-blocks/wave7"
        if os.path.exists(bb_src):
            bb_dest = os.path.join(temp_dir, "building-blocks")
            os.makedirs(bb_dest, exist_ok=True)
            shutil.copytree(bb_src, os.path.join(bb_dest, "wave7"))
            print("  ✅ Copied universal launcher and templates")
        
        # Step 3: Copy logs
        print("\nStep 3: Copying logs...")
        logs_dest = os.path.join(temp_dir, "logs")
        os.makedirs(logs_dest, exist_ok=True)
        
        # Copy wave7_* logs
        for item in os.listdir("logs"):
            if item.startswith("wave7_"):
                src = os.path.join("logs", item)
                dst = os.path.join(logs_dest, item)
                if os.path.isfile(src):
                    shutil.copy2(src, dst)
                elif os.path.isdir(src):
                    shutil.copytree(src, dst)
        
        # Copy phase0 logs
        phase0_src = "logs/phase0"
        if os.path.exists(phase0_src):
            shutil.copytree(phase0_src, os.path.join(logs_dest, "phase0"))
        
        print("  ✅ Copied session logs and execution logs")
        
        # Step 4: Copy Wave 7 scripts
        print("\nStep 4: Copying Wave 7 scripts and tools...")
        scripts = [
            "epic_roadmap_wave7.json",
            "relaunch_final_5_with_path_fix.py",
            "fix_epic_005_final.py",
            "launch_wave7_parallel.sh",
            "pilot_wave7_parallel.sh",
            "relaunch_stalled_epics.sh",
            "fix_exhausted_api_keys.py",
            "sync_wave7_to_local.sh",
            "package_wave7_for_local.sh",
            "package_wave7_for_local.py"
        ]
        
        for script in scripts:
            if os.path.exists(script):
                shutil.copy2(script, temp_dir)
        
        print("  ✅ Copied Wave 7 execution scripts")
        
        # Step 5: Copy Phase 0 scripts
        print("\nStep 5: Copying Phase 0 scripts (_p0_*.sh)...")
        scripts_dest = os.path.join(temp_dir, "phase0_scripts")
        os.makedirs(scripts_dest, exist_ok=True)
        
        script_count = 0
        for file in os.listdir("."):
            if file.startswith("_p0_") and file.endswith(".sh"):
                shutil.copy2(file, scripts_dest)
                script_count += 1
        
        print(f"  ✅ Copied {script_count} Phase 0 scripts")
        
        # Step 6: Create archive
        print("\nStep 6: Creating archive...")
        archive_path = os.path.join("/tmp", archive_name)
        
        with tarfile.open(archive_path, "w:gz") as tar:
            tar.add(temp_dir, arcname=os.path.basename(temp_dir))
        
        archive_size = os.path.getsize(archive_path)
        size_mb = archive_size / (1024 * 1024)
        print(f"  ✅ Created {archive_name} ({size_mb:.1f} MB)")
        
        # Step 7: Move to home directory
        print("\nStep 7: Moving archive to home directory...")
        final_path = os.path.join(home_dir, archive_name)
        shutil.move(archive_path, final_path)
        print(f"  ✅ Archive location: {final_path}")
        
        # Step 8: Cleanup
        print("\nStep 8: Cleaning up temporary directory...")
        shutil.rmtree(temp_dir)
        print("  ✅ Cleanup complete")
        
        # Summary
        print()
        print("=" * 70)
        print("Package Summary")
        print("=" * 70)
        print()
        print(f"Archive: {final_path}")
        print(f"Size: {size_mb:.1f} MB")
        print()
        print("Contents:")
        print(f"  ✅ {epic_count} EPIC-W7-* brain directories")
        print("  ✅ building-blocks/wave7/ (universal launcher)")
        print("  ✅ logs/wave7_* (session notes, status reports)")
        print("  ✅ logs/phase0/ (execution logs)")
        print(f"  ✅ {script_count} Phase 0 execution scripts")
        print("  ✅ Wave 7 tools and documentation")
        print()
        print("Download to local machine:")
        print(f"  scp malhitticrypto@VM_IP:{final_path} .")
        print()
        print("Extract on local machine:")
        print(f"  tar -xzf {archive_name}")
        print(f"  cd wave7_backup_*/")
        print("  cp -r * /path/to/universal-or-strategy/")
        print()
        print("✅ Package complete!")
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        if os.path.exists(temp_dir):
            shutil.rmtree(temp_dir)
        raise

if __name__ == "__main__":
    main()

# Made with Bob
