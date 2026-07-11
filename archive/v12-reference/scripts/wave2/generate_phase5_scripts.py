#!/usr/bin/env python3
"""
Generate Phase 5 (Ticket Execution + Validation) scripts following SOP building blocks method.

Architecture: Gated Sequential Workflow
- 30 ticket execution scripts (1 per ticket)
- 30 validator scripts (1 per ticket)
- 7 epic review scripts (1 per epic)
- 1 gated launcher (enforces sequential execution with validation gates)

CRITICAL: This follows WAVE_PHASE_SCRIPT_GENERATION_SOP.md
- Copy Phase 4 pattern (don't generate from scratch)
- Change only phase-specific content
- Keep API keys, structure, invocation pattern identical
"""

import re
from pathlib import Path

# Epic IDs and their ticket counts (from Phase 4 outputs)
EPIC_TICKETS = {
    107: 6,
    108: 5,
    109: 4,
    111: 3,  # Option B (original scope)
    112: 6,
    113: 5,
    114: 1
}

def copy_and_modify_phase4_to_phase5_ticket(epic_id, ticket_num):
    """Copy Phase 4 script and modify for Phase 5 ticket execution."""
    phase4_script = Path(f"_p4_{epic_id}.sh")
    if not phase4_script.exists():
        raise FileNotFoundError(f"Phase 4 baseline not found: {phase4_script}")
    
    content = phase4_script.read_text()
    content = content.replace("phase4", "phase5")
    content = content.replace("Phase 4", "Phase 5")
    content = content.replace("logs/phase4", "logs/phase5")
    content = content.replace(
        f"/tmp/phase5_msg_{epic_id}.txt",
        f"/tmp/phase5_msg_{epic_id}_t{ticket_num}.txt"
    )
    content = content.replace(
        f"logs/phase5/EPIC-CCN-{epic_id}.log",
        f"logs/phase5/EPIC-CCN-{epic_id}-T{ticket_num}.log"
    )
    
    task_description = f"""You are executing TICKET-{ticket_num} for EPIC-CCN-{epic_id}.

**Input**: Read `docs/brain/EPIC-CCN-{epic_id}/04-tickets.md`, locate TICKET-{ticket_num}

**Task**: Execute TICKET-{ticket_num} with self-validation (Tier 1).

**Steps**: 1) Read ticket spec 2) Implement code 3) Write tests 4) Run tests 5) Self-validate 6) Create completion report

**Output**: `docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-completion.md` with self-validation results

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.{ticket_num} (Ticket Execution + Self-Validation)"""
    
    content = re.sub(
        r"(cat > /tmp/phase5_msg_\d+(_t\d+)?\.txt << 'EOFMSG'\n).*?(\nEOFMSG)",
        r"\1" + task_description + r"\3",
        content,
        flags=re.DOTALL
    )
    
    content = content.replace('bob --yolo --chat-mode plan', 'bob --yolo --chat-mode v12-engineer')
    
    output_file = Path(f"_p5_{epic_id}_t{ticket_num}.sh")
    output_file.write_text(content)
    return output_file

def copy_and_modify_phase4_to_phase5_validator(epic_id, ticket_num):
    """Copy Phase 4 script and modify for Phase 5 ticket validation."""
    phase4_script = Path(f"_p4_{epic_id}.sh")
    if not phase4_script.exists():
        raise FileNotFoundError(f"Phase 4 baseline not found: {phase4_script}")
    
    content = phase4_script.read_text()
    content = content.replace("phase4", "phase5v")
    content = content.replace("Phase 4", "Phase 5.V")
    content = content.replace("logs/phase4", "logs/phase5v")
    content = content.replace(
        f"/tmp/phase5v_msg_{epic_id}.txt",
        f"/tmp/phase5v_msg_{epic_id}_t{ticket_num}.txt"
    )
    content = content.replace(
        f"logs/phase5v/EPIC-CCN-{epic_id}.log",
        f"logs/phase5v/EPIC-CCN-{epic_id}-T{ticket_num}-VALIDATION.log"
    )
    
    task_description = f"""You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-{ticket_num} of EPIC-CCN-{epic_id}.

**Input**: Read `docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-{ticket_num} implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.{ticket_num}.V (Independent Ticket Validation)"""
    
    content = re.sub(
        r"(cat > /tmp/phase5v_msg_\d+(_t\d+)?\.txt << 'EOFMSG'\n).*?(\nEOFMSG)",
        r"\1" + task_description + r"\3",
        content,
        flags=re.DOTALL
    )
    
    content = content.replace('bob --yolo --chat-mode plan', 'bob --yolo --chat-mode advanced')
    
    output_file = Path(f"_p5v_{epic_id}_t{ticket_num}.sh")
    output_file.write_text(content)
    return output_file

def copy_and_modify_phase4_to_phase6_review(epic_id):
    """Copy Phase 4 script and modify for Phase 6 epic review."""
    phase4_script = Path(f"_p4_{epic_id}.sh")
    if not phase4_script.exists():
        raise FileNotFoundError(f"Phase 4 baseline not found: {phase4_script}")
    
    content = phase4_script.read_text()
    content = content.replace("phase4", "phase6")
    content = content.replace("Phase 4", "Phase 6")
    content = content.replace("logs/phase4", "logs/phase6")
    
    ticket_count = EPIC_TICKETS[epic_id]
    task_description = f"""You are performing EPIC-LEVEL REVIEW (Tier 3) for EPIC-CCN-{epic_id}.

**Input**: Read all ticket verification reports and completion reports

**Task**: Review entire epic ({ticket_count} tickets) for integration, consistency, and overall quality.

**Steps**: 1) Verify all tickets passed 2) Check integration 3) Verify architecture 4) Run full test suite 5) Provide final verdict

**Output**: `docs/brain/EPIC-CCN-{epic_id}/05-completion-report.md` with epic verdict

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 6 (Epic-Level Review)"""
    
    content = re.sub(
        r"(cat > /tmp/phase6_msg_\d+\.txt << 'EOFMSG'\n).*?(\nEOFMSG)",
        r"\1" + task_description + r"\2",
        content,
        flags=re.DOTALL
    )
    
    content = content.replace('bob --yolo --chat-mode plan', 'bob --yolo --chat-mode advanced')
    
    output_file = Path(f"_p6_{epic_id}.sh")
    output_file.write_text(content)
    return output_file

def generate_gated_launcher():
    """Generate gated sequential launcher script."""
    launcher_content = """#!/bin/bash
# Gated Sequential Workflow: TICKET -> VALIDATE -> TICKET -> VALIDATE

wait_for_completion() {
    while screen -list | grep -q "$1"; do sleep 10; done
}

check_validation_result() {
    local verification_file="docs/brain/EPIC-CCN-$1/ticket-$2-verification.md"
    if grep -q "Verdict.*FAIL" "$verification_file"; then
        echo "[FAIL] TICKET-$2 validation FAILED. Fix before proceeding."
        return 1
    fi
    return 0
}

"""
    
    for epic_id, ticket_count in EPIC_TICKETS.items():
        launcher_content += f"\n# EPIC-CCN-{epic_id}\n"
        for ticket_num in range(1, ticket_count + 1):
            launcher_content += f"""screen -dmS p5_{epic_id}_t{ticket_num} bash -l _p5_{epic_id}_t{ticket_num}.sh
wait_for_completion p5_{epic_id}_t{ticket_num}
screen -dmS p5v_{epic_id}_t{ticket_num} bash -l _p5v_{epic_id}_t{ticket_num}.sh
wait_for_completion p5v_{epic_id}_t{ticket_num}
check_validation_result {epic_id} {ticket_num} || exit 1
"""
        launcher_content += f"screen -dmS p6_{epic_id} bash -l _p6_{epic_id}.sh\nwait_for_completion p6_{epic_id}\n"
    
    Path("launch_phase5_gated.sh").write_text(launcher_content)

def main():
    print("=== Phase 5 Script Generation ===")
    
    ticket_scripts = []
    validator_scripts = []
    review_scripts = []
    
    for epic_id, ticket_count in EPIC_TICKETS.items():
        for ticket_num in range(1, ticket_count + 1):
            ticket_scripts.append(copy_and_modify_phase4_to_phase5_ticket(epic_id, ticket_num))
            validator_scripts.append(copy_and_modify_phase4_to_phase5_validator(epic_id, ticket_num))
        review_scripts.append(copy_and_modify_phase4_to_phase6_review(epic_id))
    
    generate_gated_launcher()
    
    print(f"[OK] {len(ticket_scripts)} ticket scripts")
    print(f"[OK] {len(validator_scripts)} validator scripts")
    print(f"[OK] {len(review_scripts)} review scripts")
    print("[OK] 1 gated launcher")

if __name__ == "__main__":
    main()

# Made with Bob
