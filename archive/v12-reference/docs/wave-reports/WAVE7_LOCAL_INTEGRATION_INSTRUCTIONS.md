# Wave 7 Phase 0 - Local Backup Integration

**COPY THIS ENTIRE MESSAGE AND PASTE INTO YOUR LOCAL BOB IDE:**

---

# Task: Integrate Wave 7 Phase 0 Backup from VM

## Context
Wave 7 Phase 0 is complete on VM (161/161 epics). Backup archive ready at:
`/home/malhitticrypto/wave7_phase0_complete_20260623_041615.tar.gz` (1.1 MB)

## Step 1: Download Archive
```bash
# Ask user for VM IP, then run:
scp malhitticrypto@VM_IP:/home/malhitticrypto/wave7_phase0_complete_20260623_041615.tar.gz .
```

## Step 2: Extract Archive
```bash
tar -xzf wave7_phase0_complete_20260623_041615.tar.gz
cd wave7_backup_20260623_041615/
```

## Step 3: Integrate Files
```bash
# Copy epic directories (161 total)
cp -r docs/brain/EPIC-W7-* /path/to/universal-or-strategy/docs/brain/

# Copy building-blocks templates
cp -r building-blocks/wave7/* /path/to/universal-or-strategy/building-blocks/wave7/

# Copy logs
cp -r logs/wave7_* /path/to/universal-or-strategy/logs/
cp -r logs/phase0 /path/to/universal-or-strategy/logs/

# Copy scripts
cp _p0_*.sh /path/to/universal-or-strategy/
cp *.py /path/to/universal-or-strategy/
```

## Step 4: Verify Integration
```bash
cd /path/to/universal-or-strategy

# Should show 161
ls -d docs/brain/EPIC-W7-* | wc -l

# Should show no output (all present)
for i in $(seq -f '%03g' 1 161); do
  [ ! -f "docs/brain/EPIC-W7-$i/00-hotspots.md" ] && echo "Missing: EPIC-W7-$i"
done
```

## Step 5: Report Status
Create verification report and show git status.

## Success Criteria
- ✅ 161 epic directories in docs/brain/
- ✅ Each has 00-hotspots.md + manifest.json
- ✅ Building-blocks templates present
- ✅ Ready for Phase 1

## Questions for User
1. What is the VM IP address?
2. What is the local repo path?

---

**After completion, report verification results.**