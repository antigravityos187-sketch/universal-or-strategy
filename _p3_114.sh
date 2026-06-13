#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_65hPWuoJAPhLQKgnKSePPDiqS5YRKW1XDF1LM8kRporvu9XTpgAaY4WYvJgAe72VzRDARKEQzqzMei9UqCj28buk_2Astcnxpem897Pn91xpJXnKY6N7dMhDXAriwNtncfzsB'
mkdir -p docs/brain/EPIC-CCN-114
mkdir -p logs/phase3

cat > /tmp/phase3_msg_114.txt << 'EOFMSG'
You are executing Phase 3 (DNA & PR Audit) for EPIC-CCN-114.

**Input Artifact**: Read `docs/brain/EPIC-CCN-114/02-architecture-plan.md` for architecture plan.

**Your Task**: Perform V12 DNA compliance checks and PR hygiene validation.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-114/03-audit-report.md` with:
   - V12 DNA compliance checks (lock-free, ASCII-only, Jane Street alignment)
   - PR hygiene validation (diff size, whitespace, scope creep)
   - Pre-flight safety checks
   - Risk assessment
   - Go/No-Go recommendation

2. Update `docs/brain/EPIC-CCN-114/manifest.json`:
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

bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_114.txt)" 2>&1 | tee logs/phase3/EPIC-CCN-114.log
echo "DONE_EXIT=$?"

# Made with Bob
