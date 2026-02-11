---
description: How to detect and resolve NinjaTrader 8 "OneDrive Ghosting" freezes
---

# OneDrive Ghost Resolution Workflow

This workflow is used when NinjaTrader 8 freezes on the "Log In" splash screen or exhibits silent startup hangs.

## 1. Detection
Identify if Windows is redirecting your documents to OneDrive.
```powershell
Get-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" -Name "Personal"
```
Check for "Ghost" folders in OneDrive:
```powershell
Test-Path "C:\Users\Mohammed Khalid\OneDrive\Documents\NinjaTrader 8"
```

## 2. The Isolation Routine
If a ghost folder exists:
1. **Kill NinjaTrader**: `taskkill /F /IM NinjaTrader.exe`
2. **Move OneDrive Custom Files**: 
   Move all files from `C:\Users\Mohammed Khalid\OneDrive\Documents\NinjaTrader 8\bin\Custom` to a backup on the Desktop.
3. **Purge OneDrive Cache**:
   Delete `C:\Users\Mohammed Khalid\OneDrive\Documents\NinjaTrader 8\cache\*`
4. **Kill OneDrive (Optional)**: `taskkill /F /IM OneDrive.exe`

## 3. Permanent Fix
1. Ensure all development code is saved in the **Local** folder: `C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom`.
2. If possible, Right-click the `NinjaTrader 8` folder in OneDrive and select **"Always keep on this device"** and then **Exclue from Sync** in OneDrive settings to prevent locking.

## 4. Verification
Start NinjaTrader. If it loads to the Control Center, the "Ghost" has been busted.
