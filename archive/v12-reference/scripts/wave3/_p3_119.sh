#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX'
mkdir -p docs/brain/EPIC-CCN-119
mkdir -p logs/phase3

cat > /tmp/phase3_msg_119.txt << 'EOFMSG'
You are executing Phase 3 (DNA & PR Audit) for EPIC-CCN-119.

**Input Artifact**: Read `docs/brain/EPIC-CCN-119/02-implementation-plan.md` for architecture plan.

**Your Task**: Perform V12 DNA compliance checks and PR hygiene validation.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-119/03-audit-report.md` with:
   - V12 DNA compliance checks (lock-free, ASCII-only, Jane Street alignment)
   - PR hygiene validation (diff size, whitespace, scope creep)
   - Pre-flight safety checks
   - Risk assessment
   - Go/No-Go recommendation

2. Update `docs/brain/EPIC-CCN-119/manifest.json`:
   - Set phase "3" status to "completed"
   - Add "03-audit-report.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Target complexity <= 8 (Jane Street alignment)
- Check for lock-free compliance (no lock() statements)
- Verify ASCII-only (no Unicode/emoji)
- Validate PR diff < 10k characters

**Phase**: 3 (DNA & PR Audit)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_119.txt)" 2>&1 | tee logs/phase3/EPIC-CCN-119.log
echo "DONE_EXIT=$?"

# Made with Bob
