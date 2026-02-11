---
name: nt8-backup-manager
description: Manages NinjaTrader 8 local backups and ensures path stability after OneDrive removal.
---

# NT8 Backup Manager Skill

This skill provides a standardized way to back up NinjaTrader 8 settings and verify the local path configuration.

## Core Responsibilities
1. **Path Verification**: Ensure `C:\Users\Mohammed Khalid\Documents\NinjaTrader 8` is the active directory.
2. **Automated Backup**: Create timestamped backups of Templates, Workspaces, and Configuration files.
3. **OneDrive Ghost Audit**: Specifically check for `Documents` redirection and secondary data paths.
4. **Registry Verification**: Confirm Windows is using local `Documents`, not the OneDrive virtual path.

## Usage

### Run Backup
Use the included PowerShell script to create a snapshot on the Desktop:
```powershell
powershell -File ".agent/skills/nt8-backup-manager/scripts/backup.ps1"
```

### Proactive Environment Audit (The Ghost Hunter)
Run this whenever NinjaTrader hangs or shows "Assembly Conflict":
```powershell
# Check for redirection
Get-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" -Name "Personal"

# Check for ghost folders
Test-Path "C:\Users\Mohammed Khalid\OneDrive\Documents\NinjaTrader 8"
```

## Critical Locations
- **Local Data (SAFE)**: `C:\Users\Mohammed Khalid\Documents\NinjaTrader 8`
- **OneDrive Proxy (RISK)**: `C:\Users\Mohammed Khalid\OneDrive\Documents\NinjaTrader 8`
- **Backup Root**: `C:\Users\Mohammed Khalid\OneDrive\Desktop\WSGTA\NinjaTrader_Local_Backups`

## The "OneDrive Ghost" Rule
**NEVER** allow custom `.cs` or `.dll` files to exist in the OneDrive path while NinjaTrader is running.
1. If a file is in OneDrive, NinjaTrader will try to scan it twice (Local + Cloud).
2. Sync latency on the `.sqlite` database or `Config.xml` will cause a **silent freeze** on startup.
3. **Resolution**: Always Move files to Local `Documents` and exclude `Documents\NinjaTrader 8` from OneDrive sync if possible.

## Troubleshooting
If NinjaTrader shows "Path not found" or "File in use":
1. Check Registry: `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders` -> `Personal` should be local.
2. Kill OneDrive: `taskkill /F /IM OneDrive.exe /T`
3. Clear Recovery: Remove `bin\Custom\Recovery` if compilation fails.
4. Purge Ghost: Delete or move `OneDrive\Documents\NinjaTrader 8\bin\Custom`.
