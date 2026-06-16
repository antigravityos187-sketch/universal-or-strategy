# V12 Wave Launcher - Complete Guide

## The Solution: GCP Startup Script via VM Metadata

### Why This Solves the Quote Escaping Problem

```
BEFORE (6 layers of quote hell):
  PowerShell → gcloud CLI → SSH wire → bash -c → screen → bash → Bob
  Layer 1        Layer 2     Layer 3    Layer 4   Layer 5  Layer 6 Layer 7
  ❌ FAILS at every transition

AFTER (0 transport layers for the script):
  PowerShell → gcloud instances create --metadata=startup-script=<file>
  Layer 1        Layer 2 (gcloud handles encoding natively)
  ✅ Script runs directly on VM as root at boot - zero quote issues
```

GCP's `startup-script` metadata key is designed exactly for this: it takes the raw script content, base64-encodes it internally, and GCP's metadata agent decodes and executes it on the VM. **No shell ever sees the script as a string argument.**

---

## Quick Start

### Prerequisites
1. Python 3.10+
2. gcloud CLI authenticated (`gcloud auth login`)
3. Bob API key (set as env var or in `.env` file)

### Set Up Bob API Key
```powershell
# Option A: Environment variable (session)
$env:BOB_API_KEY = "your-ibm-api-key-here"

# Option B: .env file (persistent, gitignored)
Add-Content .env "BOB_API_KEY=your-ibm-api-key-here"
```

### Launch Wave 2 (Single Command)
```powershell
# Using epic list file
python scripts/wave2/launch_wave.py --wave 2 --epics-file scripts/wave2/wave2_epics.txt

# Or inline
python scripts/wave2/launch_wave.py --wave 2 --epics "EPIC-CCN-164,EPIC-CCN-107,EPIC-CCN-108"

# Dry run first (prints what would happen, no charges)
python scripts/wave2/launch_wave.py --wave 2 --epics-file scripts/wave2/wave2_epics.txt --dry-run
```

### Monitor Progress
```powershell
# After launch prints the VM name (e.g., v12-wave2-20260612-090000):
python scripts/wave2/launch_wave.py --monitor v12-wave2-20260612-090000
```

### Collect Results
```powershell
python scripts/wave2/launch_wave.py --collect v12-wave2-20260612-090000
# Results saved to docs/brain/wave-results/
```

---

## How It Works (Architecture)

```
Windows Laptop                    GCP
──────────────────────────────────────────────────────────
1. python launch_wave.py          
   reads orchestrator.sh ──────► gcloud instances create
   reads wave2_epics.txt          --metadata=startup-script=<script>
   reads BOB_API_KEY              --metadata=v12-epics=EPIC-CCN-164,...
                                  --metadata=v12-bob-api-key=<key>
                                            │
                                            ▼
                                   VM boots from golden image
                                            │
                                            ▼
                                   GCP metadata agent runs startup-script
                                   (as root, no SSH, no quote issues)
                                            │
                                            ▼
                                   orchestrator.sh:
                                   1. reads epics from metadata
                                   2. sets global git identity
                                   3. pulls latest repo
                                   4. configures Bob API key
                                   5. launches N screen sessions in parallel
                                   
                                   screen v12-EPIC-CCN-164 ──► bob run epic
                                   screen v12-EPIC-CCN-107 ──► bob run epic
                                   screen v12-EPIC-CCN-108 ──► bob run epic
                                   ...
```

---

## Golden Image v2 Requirements (Bob Pre-Auth)

For Bob Shell to run headlessly, the golden image must be created with Bob
authenticated via API key. Steps to create golden image v3 (if needed):

```bash
# On the golden VM (via Google Cloud Console browser SSH):
bob --auth-method api-key --api-key "YOUR_IBM_API_KEY"
# Accept license, verify auth works
# Then create image snapshot

gcloud compute images create v12-bob-shell-golden-v3 \
  --source-disk=GOLDEN_VM_NAME \
  --source-disk-zone=us-central1-a
```

Alternatively, the orchestrator.sh script injects the API key via metadata at
startup - so the golden image doesn't need pre-baked auth.

---

## Cost Analysis

| Component | Cost |
|-----------|------|
| Wave VM (n2-standard-8 SPOT, ~30 min) | ~$0.047 |
| Startup script metadata | FREE |
| Python launcher | FREE |
| Monitoring SSH calls | negligible |
| **Total per Wave** | **~$0.05** |

No additional GCP services needed. No Cloud Run, no Cloud Functions, no Cloud Build.

---

## Maintenance Burden

**Very low.** To run Wave 3:
1. Edit `scripts/wave2/wave2_epics.txt` with new epic IDs
2. Run the same launch command with `--wave 3`

To change parallelism: just add/remove lines from the epics file.
To change the orchestrator logic: edit `scripts/wave2/orchestrator.sh`.

---

## Troubleshooting

### Check startup script logs
```powershell
gcloud compute ssh v12-wave2-<timestamp> --zone=us-central1-a --command="sudo journalctl -u google-startup-scripts -f"
# Or:
gcloud compute ssh v12-wave2-<timestamp> --zone=us-central1-a --command="sudo cat /var/log/v12-orchestrator.log"
```

### Check running screen sessions
```powershell
gcloud compute ssh v12-wave2-<timestamp> --zone=us-central1-a --command="sudo -u malhitticrypto screen -ls"
```

### Check individual epic log
```powershell
gcloud compute ssh v12-wave2-<timestamp> --zone=us-central1-a --command="tail -50 ~/universal-or-strategy/logs/EPIC-CCN-164.log"
```

### VM not starting (SPOT preemption)
SPOT VMs can be preempted. If the VM disappears, re-run the launch command.
Consider `--provisioning-model=STANDARD` for critical waves (3x cost).
