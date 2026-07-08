# Wave 7 Quick Reference Card

**Version**: 1.0  
**Purpose**: Quick command reference for Wave 7 execution

---

## 🚀 Launch Commands

```bash
# Launch full wave (161 epics)
bash scripts/launch_wave7.sh

# Check status anytime
bash scripts/check_wave7_status.sh

# Watch status continuously (every 4 minutes)
watch -n 240 bash scripts/check_wave7_status.sh
```

---

## 📊 Monitoring Commands

```bash
# Overall progress
grep '"event_type": "epic_complete"' .lamport/wave7/event_log.jsonl | wc -l

# Failed epics
grep '"event_type": "phase_fail"' .lamport/wave7/event_log.jsonl

# Active VM sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# VM disk usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="df -h"
```

---

## 🔍 Debugging Commands

```bash
# View specific epic log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/EPIC-W7-042-phase0.log"

# Search for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="grep -i 'error\|failed' /home/malhitticrypto/universal-or-strategy/logs/EPIC-W7-*.log | head -20"

# Check epic manifest
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cat /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-W7-042/manifest.json"

# View failure analysis
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cat /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-W7-042/failure-analysis.md"
```

---

## 🔧 Recovery Commands

```bash
# Kill stuck session
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="screen -X -S EPIC-W7-042 quit"

# Re-launch specific epic
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto/universal-or-strategy && docs/brain/EPIC-W7-042/_phase0.sh"

# Clean up logs
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto/universal-or-strategy && rm -f logs/EPIC-W7-*.log"
```

---

## ✅ Post-Wave Commands

```bash
# Sync from VM to local
powershell -File .\deploy-sync.ps1

# Run pre-push validation
powershell -File .\scripts\pre_push_validation.ps1

# Verify complexity reduction
python scripts/complexity_audit.py

# Compare before/after
diff complexity_audit_fresh_2026-06-14.txt complexity_audit_wave7_complete.txt
```

---

## 📈 Progress Tracking

| Metric | Command |
|--------|---------|
| Completed epics | `grep '"event_type": "epic_complete"' .lamport/wave7/event_log.jsonl \| wc -l` |
| Failed epics | `grep '"event_type": "phase_fail"' .lamport/wave7/event_log.jsonl \| wc -l` |
| Running sessions | `gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls \| grep -c EPIC-W7"` |
| Completion % | `echo "scale=2; $(grep -c epic_complete .lamport/wave7/event_log.jsonl) * 100 / 161" \| bc` |

---

## 🎯 Success Criteria Checklist

- [ ] 161/161 epics complete
- [ ] Zero failed epics
- [ ] Build passes on VM
- [ ] Sync to local successful
- [ ] Pre-push validation passes (13/13)
- [ ] All methods CYC ≤8
- [ ] F5 in NinjaTrader succeeds
- [ ] Zero UTF-8 violations
- [ ] Zero xUnit framework violations

---

## 🆘 Emergency Commands

```bash
# Stop all epics (emergency shutdown)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="screen -ls | grep EPIC-W7 | cut -d. -f1 | xargs -I {} screen -X -S {} quit"

# Backup current state
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto && tar -czf wave7-backup-$(date +%Y%m%d-%H%M%S).tar.gz universal-or-strategy/docs/brain/EPIC-W7-*"

# Restore from backup
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
    --command="cd /home/malhitticrypto && tar -xzf wave7-backup-YYYYMMDD-HHMMSS.tar.gz"
```

---

## 📞 Support

- **Execution Plan**: `docs/workflow/WAVE7_EXECUTION_PLAN.md`
- **Full README**: `scripts/WAVE7_LAUNCH_README.md`
- **Polling Protocol**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Lamport Events**: `.lamport/wave7/README.md`

---

**Last Updated**: 2026-06-21  
**Version**: 1.0