---
description: How to setup and run Rovo Dev CLI
---

# Rovo Dev CLI Setup Workflow

This workflow guides you through the setup and execution of the Rovo Dev CLI.

## Prerequisites
- `acli.exe` must be in the project root.
- A scoped API token for Rovo Dev.

## Setup Steps

1. **Create API Token**
   - Visit the [scoped API token creation link](https://id.atlassian.com/manage-profile/security/api-tokens?autofillToken&expiryDays=max&appId=rovodev&selectedScopes=all).
   - Create and copy the token.

2. **Authenticate & Rotate**
   - The API token is stored in the [`.env`](file:///C:/WSGTA/universal-or-strategy/.env) file.
   - When the token expires, update `ROVO_API_TOKEN` in `.env`.
   - Run the login command (using the email `malhitti@gmail.com`):
   ```powershell
   $token = Select-String -Path .env -Pattern "ROVO_API_TOKEN=(.*)" | % { $_.Matches.Groups[1].Value }
   echo "malhitti@gmail.com" | .\acli.exe rovodev auth login --token $token
   ```


3. **Run Rovo Dev**
   - Once authorized, run:
   ```powershell
   .\acli.exe rovodev run
   ```

## Troubleshooting
- If `acli.exe` is blocked by Windows, right-click the file -> Properties -> Unblock.
- Ensure PowerShell execution policy allows running local scripts if needed (though `acli.exe` is a binary).
