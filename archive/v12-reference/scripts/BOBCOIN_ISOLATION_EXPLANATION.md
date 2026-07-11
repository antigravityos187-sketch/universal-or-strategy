# Bobcoin Isolation: VM vs Chat Session

**Question**: Will Bob agents on the VM keep running if this chat session runs out of bobcoins?

**Answer**: ✅ **YES - VM agents are completely isolated from this chat session's bobcoin balance.**

---

## Architecture Overview

### Two Separate Bobcoin Pools

**1. Chat Session (Claude/This Conversation)**
- **API Key**: Your Claude API key (Anthropic)
- **Balance**: Separate from Bob Shell
- **Usage**: Powers this conversation only
- **Impact if depleted**: This chat stops, but VM agents continue

**2. VM Bob Shell Agents (80 Parallel Agents)**
- **API Keys**: 15 Bob Shell API keys (stored in `docs/API/*.json`)
- **Balance**: 160 bobcoins each = 2,400 total
- **Usage**: Powers Bob Shell agents on VM only
- **Impact if depleted**: VM agents stop, but this chat continues

### Complete Isolation

```
┌─────────────────────────────────────────────────────────────┐
│                     LOCAL MACHINE                            │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Claude Chat Session (This Conversation)           │    │
│  │  - Uses: Anthropic API Key                         │    │
│  │  - Balance: Your Claude credits                    │    │
│  │  - Role: Orchestration, monitoring, reporting      │    │
│  └────────────────────────────────────────────────────┘    │
│                          │                                   │
│                          │ SSH Commands                      │
│                          │ (gcloud compute ssh)              │
│                          ▼                                   │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ Network
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                    GCP VM (v12-test-golden-v2)              │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  80 Bob Shell Agents (Screen Sessions)            │    │
│  │  - Uses: 15 Bob Shell API Keys (round-robin)      │    │
│  │  - Balance: 2,400 bobcoins total                  │    │
│  │  - Role: Execute Phase 2 architecture planning    │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  API Keys Stored Locally on VM:                             │
│  - /home/malhitticrypto/universal-or-strategy/docs/API/     │
│  - bob.json, bob (1).json, ..., bob (6).json               │
│  - jessica.json, mikethelife.json, etc.                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## What Happens If This Chat Runs Out of Bobcoins?

### Scenario: Claude Chat Depletes

**Impact on This Chat**:
- ❌ This conversation stops responding
- ❌ Cannot send new SSH commands
- ❌ Cannot monitor VM progress
- ❌ Cannot create reports

**Impact on VM Agents**:
- ✅ **VM agents continue running** (completely isolated)
- ✅ All 80 epics continue executing
- ✅ Files continue being created
- ✅ Bob Shell API keys unaffected

**Recovery**:
1. Reload this chat with more bobcoins
2. Resume monitoring with SSH commands
3. VM agents never knew you were gone

---

## What Happens If VM Bob Shell APIs Run Out?

### Scenario: VM API Keys Deplete

**Impact on VM Agents**:
- ❌ VM agents stop when their API key hits 0 bobcoins
- ❌ Affected epics fail with "Insufficient credits" error
- ❌ Files not created for failed epics

**Impact on This Chat**:
- ✅ **This conversation continues** (separate API)
- ✅ Can still monitor VM
- ✅ Can detect failures via SSH
- ✅ Can create failure reports

**Recovery**:
1. Add bobcoins to depleted Bob Shell API keys
2. Relaunch failed epics
3. This chat continues orchestrating

---

## Current Budget Status

### VM Bob Shell APIs (Phase 2)

**Total Available**: 2,400 bobcoins (15 APIs × 160 each)
**Phase 2 Budget**: 1,775 bobcoins (80 epics)
**Safety Margin**: 26% (625 bobcoins)

**Pilot Test Usage**:
- Pilot #1: 2.68 bobcoins
- Pilot #2: 2.84 bobcoins
- Average: 2.76 bobcoins per epic

**Projected Usage**: 80 × 2.76 = 220.8 bobcoins (87.6% under budget)

**Risk**: ✅ **VERY LOW** - Massive safety margin

### This Chat Session

**Your Question Implies**: You're monitoring this chat's bobcoin usage
**Current Cost**: $156.28 (from environment details)
**Impact on VM**: ❌ **NONE** - Separate API pools

---

## API Rotation Strategy (VM)

### How VM Agents Use APIs

**Round-Robin Allocation**:
- API-1: Epics 001, 016, 031, 046, 061, 076 (6 epics)
- API-2: Epics 002, 017, 032, 047, 062, 077 (6 epics)
- API-3: Epics 003, 018, 033, 048, 063, 078 (6 epics)
- ...
- API-15: Epics 015, 030, 045, 060, 075 (5 epics)

**Load Balancing**: Each API handles 5-6 epics (not 80)

**Isolation**: If API-1 depletes, only 6 epics fail (not all 80)

---

## Monitoring Strategy

### While This Chat Has Bobcoins

**You Can**:
- ✅ Send SSH commands to check VM status
- ✅ Monitor file creation progress
- ✅ Extract bobcoin usage from logs
- ✅ Create completion reports
- ✅ Analyze results

### If This Chat Runs Out

**You Can**:
1. Reload this chat with more bobcoins
2. Resume monitoring immediately
3. VM agents never stopped (they're isolated)
4. Catch up on progress via SSH commands

**You Cannot**:
- ❌ Send new SSH commands (chat is frozen)
- ❌ Create new reports (chat is frozen)
- ❌ Respond to VM issues (chat is frozen)

**Workaround**: Use manual SSH from terminal:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a
cd /home/malhitticrypto/universal-or-strategy
screen -ls  # Check running agents
ls docs/brain/EPIC-CCN-*/02-architecture-plan.md | wc -l  # Count files
```

---

## Best Practices

### For This Chat Session

1. **Monitor bobcoin usage** (shown in environment details)
2. **Reload before depletion** (don't wait until 0)
3. **Keep monitoring commands ready** (can resume anytime)

### For VM Bob Shell APIs

1. **Track usage per API** (extract from logs)
2. **Monitor safety margin** (should stay >10%)
3. **Have backup APIs ready** (can rotate if needed)

### Emergency Protocol

**If This Chat Depletes Mid-Wave**:
1. Don't panic - VM agents continue
2. Reload chat with bobcoins
3. Resume monitoring: `gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"`
4. Check progress: `ls docs/brain/EPIC-CCN-*/02-architecture-plan.md | wc -l`
5. Continue from where you left off

**If VM APIs Deplete Mid-Wave**:
1. Identify depleted API (check logs)
2. Add bobcoins to that API key
3. Relaunch failed epics
4. This chat continues orchestrating

---

## Summary

**Your Question**: Will VM Bob agents keep running if this chat runs out of bobcoins?

**Answer**: ✅ **YES - Completely isolated API pools.**

**Key Points**:
- This chat uses Anthropic API (your Claude credits)
- VM agents use Bob Shell APIs (15 separate keys, 2,400 bobcoins)
- Depletion of one does NOT affect the other
- VM agents will complete even if this chat stops
- You can reload this chat and resume monitoring anytime

**Current Status**:
- VM agents: ✅ Running (23/80 launched, 57 remaining)
- This chat: ✅ Active ($156.28 used)
- Risk: ✅ LOW (both pools have large safety margins)

---

**Document Status**: REFERENCE
**Purpose**: Explain bobcoin isolation architecture
**Audience**: Wave 4 execution team
**Date**: 2026-06-15