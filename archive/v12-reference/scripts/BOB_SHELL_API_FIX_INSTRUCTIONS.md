# Bob Shell API Key Fix - Complete Instructions

**Date**: 2026-06-14
**Issue**: Bob Shell showing old depleted balance (160.08/160)
**Root Cause**: Bob Shell cached old API key when VSCode started

---

## ✅ What Has Been Fixed

1. **Settings file corrected**: `.bob/settings.json` now has correct `"api_key"` field
2. **Environment variable removed**: `BOBSHELL_API_KEY` removed from USER-level environment (permanent)
3. **Verification complete**: Environment variable confirmed removed

**Verification**:
```powershell
[System.Environment]::GetEnvironmentVariable('BOBSHELL_API_KEY', 'User')
# Output: (empty) ✅
```

---

## 🔄 Why You Still See Old Balance

**Bob Shell caches authentication when VSCode starts.** The current VSCode window loaded Bob Shell with the old API key before we removed the environment variable. Bob Shell won't reload the new API key until you restart VSCode.

---

## 📋 Required Steps to See New Balance

### Option 1: Restart VSCode (Recommended)

1. **Save all your work**
2. **Close VSCode completely** (File → Exit, or Alt+F4)
3. **Reopen VSCode**
4. **Open a terminal** and verify:
   ```powershell
   echo $env:BOBSHELL_API_KEY
   ```
   Should output: **(empty/nothing)**
5. **Start Bob Shell** - you should now see your fresh balance

### Option 2: Reload Window (Faster)

1. **Press**: `Ctrl+Shift+P`
2. **Type**: "Developer: Reload Window"
3. **Press**: Enter
4. **Open a terminal** and verify environment variable is empty
5. **Start Bob Shell** - should show fresh balance

---

## 🔍 How to Verify It Worked

After restarting VSCode:

1. **Check environment variable** (should be empty):
   ```powershell
   echo $env:BOBSHELL_API_KEY
   ```

2. **Check settings file** (should have api_key):
   ```powershell
   Get-Content .bob/settings.json | Select-String "api_key"
   ```
   Should show: `"api_key": "bob_prod_bob-admin_yN7cbW..."`

3. **Start Bob Shell** and look at status bar - should show fresh balance

---

## 📊 API Key Details

**Old API** (removed):
- Key: `bob_prod_bob-admin_2DNk7b...`
- Balance: 160.08/160 (depleted)
- Location: USER environment variable (now removed)

**New API** (active):
- Key: `bob_prod_bob-admin_yN7cbW...`
- Balance: Fresh (full allocation)
- Location: `.bob/settings.json`

---

## 🚨 If It Still Shows Old Balance After Restart

If you restart VSCode and still see 160.08/160:

1. **Check if environment variable came back**:
   ```powershell
   [System.Environment]::GetEnvironmentVariable('BOBSHELL_API_KEY', 'User')
   [System.Environment]::GetEnvironmentVariable('BOBSHELL_API_KEY', 'Machine')
   ```
   Both should be empty.

2. **Check if it's set in System environment** (requires admin):
   ```powershell
   [System.Environment]::GetEnvironmentVariable('BOBSHELL_API_KEY', 'Machine')
   ```
   If this returns a value, you need to remove it at SYSTEM level (requires admin rights).

3. **Verify settings file**:
   ```powershell
   Get-Content .bob/settings.json | ConvertFrom-Json | Select-Object api_key
   ```
   Should show your new API key.

---

## 🛠️ Scripts Created

- **`scripts/fix_bob_api_auth_permanent.ps1`**: Permanent fix (removes USER-level env var)
- **`scripts/fix_bob_api_auth.ps1`**: Session-only fix (deprecated)

---

## 📝 Summary

**Status**: ✅ Fix complete - environment variable removed permanently
**Action Required**: Restart VSCode to reload Bob Shell with new API key
**Expected Result**: Bob Shell will show fresh balance from `.bob/settings.json`

---

**Last Updated**: 2026-06-14T00:03:00Z