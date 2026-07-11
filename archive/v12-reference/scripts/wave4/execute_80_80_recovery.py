#!/usr/bin/env python3
"""
Wave 4 Recovery - Achieve 80/80 Completion

Root causes identified:
1. 7 Phase 5 scripts never uploaded to VM (003, 015, 030, 031, 033, 042, 055)
2. 3 Phase 6 PATH issues (012, 027, 045) - need absolute path to bob
3. 1 scope mismatch (016) - needs manual re-scoping

This script executes Steps 1 and 2 (automated recovery).
Step 3 (EPIC-016 re-scoping) requires manual intervention.
"""

import subprocess
import time
from pathlib import Path

# Configuration
VM_NAME = "v12-test-golden-v2"
ZONE = "us-central1-a"
VM_PATH = "/home/malhitticrypto/universal-or-strategy"

# Epic lists
MISSING_P5_SCRIPTS = ["003", "015", "030", "031", "033", "042", "055"]
PATH_ISSUE_EPICS = ["012", "027", "045"]

def run_gcloud(command: str, description: str) -> tuple[int, str]:
    """Execute gcloud command and return exit code and output."""
    print(f"\n[{description}]")
    print(f"Command: {command}")
    result = subprocess.run(command, shell=True, capture_output=True, text=True)
    print(f"Exit code: {result.returncode}")
    if result.stdout:
        print(f"Output:\n{result.stdout}")
    if result.stderr:
        print(f"Stderr:\n{result.stderr}")
    return result.returncode, result.stdout

def step1_fix_phase6_path_issue():
    """Fix Phase 6 PATH issue for 3 epics."""
    print("\n" + "="*80)
    print("STEP 1: Fix Phase 6 PATH Issue (3 epics)")
    print("="*80)
    
    # Read template script
    template_path = Path("scripts/wave4/_p6_001.sh")
    if not template_path.exists():
        print(f"ERROR: Template not found: {template_path}")
        return False
    
    template = template_path.read_text()
    
    # Replace relative 'bob' with absolute path
    fixed_template = template.replace(
        'bob --yolo',
        '/home/malhitticrypto/.local/bin/bob --yolo'
    )
    
    # Generate fixed scripts for 3 epics
    for epic_num in PATH_ISSUE_EPICS:
        epic_id = f"EPIC-CCN-{epic_num}"
        
        # Read original script
        script_path = Path(f"scripts/wave4/_p6_{epic_num}.sh")
        if not script_path.exists():
            print(f"ERROR: Script not found: {script_path}")
            continue
        
        original = script_path.read_text()
        
        # Replace relative 'bob' with absolute path
        fixed = original.replace(
            'bob --yolo',
            '/home/malhitticrypto/.local/bin/bob --yolo'
        )
        
        # Write fixed script
        script_path.write_text(fixed)
        print(f"[OK] Fixed: {script_path}")
    
    # Upload fixed scripts to VM
    print("\n[Uploading fixed Phase 6 scripts to VM]")
    for epic_num in PATH_ISSUE_EPICS:
        local_path = f"scripts/wave4/_p6_{epic_num}.sh"
        remote_path = f"{VM_PATH}/scripts/wave4/_p6_{epic_num}.sh"
        
        cmd = f'gcloud compute scp {local_path} {VM_NAME}:{remote_path} --zone={ZONE}'
        code, _ = run_gcloud(cmd, f"Upload _p6_{epic_num}.sh")
        if code != 0:
            print(f"❌ Upload failed for {epic_num}")
            return False
    
    # Set permissions
    cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="chmod +x {VM_PATH}/scripts/wave4/_p6_{{012,027,045}}.sh"'
    code, _ = run_gcloud(cmd, "Set execute permissions")
    if code != 0:
        print("❌ Permission setting failed")
        return False
    
    # Launch Phase 6 recovery for 3 epics
    print("\n[Launching Phase 6 recovery for 3 epics]")
    for epic_num in PATH_ISSUE_EPICS:
        epic_id = f"EPIC-CCN-{epic_num}"
        
        cmd = f'''gcloud compute ssh {VM_NAME} --zone={ZONE} --command="cd {VM_PATH} && screen -dmS p6-{epic_num}-fix bash -l -c './scripts/wave4/_p6_{epic_num}.sh 2>&1 | tee logs/phase6/{epic_id}-recovery.log'"'''
        code, _ = run_gcloud(cmd, f"Launch Phase 6 for {epic_id}")
        
        if code == 0:
            print(f"[OK] Launched: {epic_id}")
        else:
            print(f"[ERROR] Launch failed: {epic_id}")
        
        time.sleep(12)  # Staggered launch
    
    print("\n[OK] Step 1 complete: Phase 6 recovery launched for 3 epics")
    print("Monitor with: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls | grep p6-'")
    return True

def step2_upload_missing_phase5_scripts():
    """Upload missing Phase 5 scripts and launch execution."""
    print("\n" + "="*80)
    print("STEP 2: Upload Missing Phase 5 Scripts (7 epics)")
    print("="*80)
    
    # Upload 7 missing scripts
    print("\n[Uploading 7 missing Phase 5 scripts to VM]")
    for epic_num in MISSING_P5_SCRIPTS:
        local_path = f"scripts/wave4/_p5_{epic_num}.sh"
        remote_path = f"{VM_PATH}/scripts/wave4/_p5_{epic_num}.sh"
        
        if not Path(local_path).exists():
            print(f"❌ Local script not found: {local_path}")
            return False
        
        cmd = f'gcloud compute scp {local_path} {VM_NAME}:{remote_path} --zone={ZONE}'
        code, _ = run_gcloud(cmd, f"Upload _p5_{epic_num}.sh")
        if code != 0:
            print(f"❌ Upload failed for {epic_num}")
            return False
    
    # Verify all 7 scripts uploaded
    cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="ls {VM_PATH}/scripts/wave4/_p5_{{003,015,030,031,033,042,055}}.sh | wc -l"'
    code, output = run_gcloud(cmd, "Verify 7 scripts uploaded")
    if code != 0 or "7" not in output:
        print(f"❌ Verification failed. Expected 7 scripts, got: {output}")
        return False
    
    # Set permissions
    cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="chmod +x {VM_PATH}/scripts/wave4/_p5_{{003,015,030,031,033,042,055}}.sh"'
    code, _ = run_gcloud(cmd, "Set execute permissions")
    if code != 0:
        print("❌ Permission setting failed")
        return False
    
    # Launch Phase 5 for 7 epics
    print("\n[Launching Phase 5 for 7 epics]")
    for epic_num in MISSING_P5_SCRIPTS:
        epic_id = f"EPIC-CCN-{epic_num}"
        
        cmd = f'''gcloud compute ssh {VM_NAME} --zone={ZONE} --command="cd {VM_PATH} && screen -dmS p5-{epic_num} bash -l -c './scripts/wave4/_p5_{epic_num}.sh 2>&1 | tee logs/phase5/{epic_id}.log'"'''
        code, _ = run_gcloud(cmd, f"Launch Phase 5 for {epic_id}")
        
        if code == 0:
            print(f"[OK] Launched: {epic_id}")
        else:
            print(f"[ERROR] Launch failed: {epic_id}")
        
        time.sleep(12)  # Staggered launch
    
    print("\n[OK] Step 2 complete: Phase 5 launched for 7 epics")
    print("Monitor with: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls | grep p5-'")
    return True

def monitor_recovery():
    """Monitor recovery progress."""
    print("\n" + "="*80)
    print("MONITORING RECOVERY")
    print("="*80)
    
    print("\nWait 1 minute for first check...")
    time.sleep(60)
    
    for check_num in range(1, 11):  # 10 checks, 4 minutes apart
        print(f"\n[Check {check_num}/10]")
        
        # Check Phase 6 recovery (3 epics)
        cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="cd {VM_PATH} && ls docs/brain/EPIC-CCN-{{012,027,045}}/06-verification-report.md 2>/dev/null | wc -l"'
        code, output = run_gcloud(cmd, "Phase 6 recovery progress")
        p6_count = int(output.strip()) if output.strip().isdigit() else 0
        print(f"Phase 6 recovery: {p6_count}/3 complete")
        
        # Check Phase 5 execution (7 epics)
        cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="cd {VM_PATH} && find docs/brain/EPIC-CCN-{{003,015,030,031,033,042,055}} -maxdepth 1 \\( -name \'05-*.md\' -o -name \'ticket-*-completion.md\' \\) 2>/dev/null | wc -l"'
        code, output = run_gcloud(cmd, "Phase 5 execution progress")
        p5_count = int(output.strip()) if output.strip().isdigit() else 0
        print(f"Phase 5 execution: {p5_count}/7 complete")
        
        # Check screen sessions
        cmd = f'gcloud compute ssh {VM_NAME} --zone={ZONE} --command="screen -ls | grep -E \'p5-|p6-\' | wc -l"'
        code, output = run_gcloud(cmd, "Active screen sessions")
        active = int(output.strip()) if output.strip().isdigit() else 0
        print(f"Active sessions: {active}")
        
        # Check if complete
        if p6_count == 3 and p5_count == 7 and active == 0:
            print("\n[OK] Recovery complete!")
            print("Phase 6 recovery: 3/3 [OK]")
            print("Phase 5 execution: 7/7 [OK]")
            return True
        
        if check_num < 10:
            print("\nWaiting 4 minutes for next check...")
            time.sleep(240)
    
    print("\n[WARNING] Monitoring timeout after 40 minutes")
    print("Manual check required")
    return False

def main():
    """Execute recovery plan."""
    print("="*80)
    print("WAVE 4 RECOVERY - ACHIEVE 80/80 COMPLETION")
    print("="*80)
    print("\nRoot Causes:")
    print("1. 7 Phase 5 scripts never uploaded (003, 015, 030, 031, 033, 042, 055)")
    print("2. 3 Phase 6 PATH issues (012, 027, 045)")
    print("3. 1 scope mismatch (016) - requires manual re-scoping")
    print("\nThis script handles automated recovery (Steps 1-2).")
    print("EPIC-016 requires manual intervention (Step 3).")
    
    input("\nPress Enter to start recovery...")
    
    # Step 1: Fix Phase 6 PATH issue
    if not step1_fix_phase6_path_issue():
        print("\n[ERROR] Step 1 failed. Aborting.")
        return
    
    print("\n" + "="*80)
    input("Step 1 complete. Press Enter to continue to Step 2...")
    
    # Step 2: Upload missing Phase 5 scripts
    if not step2_upload_missing_phase5_scripts():
        print("\n[ERROR] Step 2 failed. Aborting.")
        return
    
    print("\n" + "="*80)
    print("Both steps launched successfully!")
    print("\nExpected completion:")
    print("- Phase 6 recovery (3 epics): ~15 minutes")
    print("- Phase 5 execution (7 epics): ~10-15 minutes per epic")
    print("\nTotal: ~1-2 hours")
    
    monitor = input("\nMonitor progress automatically? (y/n): ")
    if monitor.lower() == 'y':
        monitor_recovery()
    else:
        print("\nManual monitoring commands:")
        print("Screen sessions: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls'")
        print("Phase 6 files: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='ls docs/brain/EPIC-CCN-{012,027,045}/06-verification-report.md'")
        print("Phase 5 files: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='find docs/brain/EPIC-CCN-{003,015,030,031,033,042,055} -name \"05-*.md\" -o -name \"ticket-*-completion.md\"'")

if __name__ == "__main__":
    main()

# Made with Bob
