# NOTIFICATION: Deployment Protocol Hardened

## Protocol: ONE SOURCE OF TRUTH

**Context:** NinjaTrader 8 (NT8) and GitHub Repository synchronization.

### 🛡️ Mandatory Rule for All Agents
1.  **NEVER** manually copy-paste code into NinjaTrader folders.
2.  **ALWAYS** edit files in the repository path: `C:\WSGTA\universal-or-strategy\`.
3.  **MANDATORY SYNC**: After any code change (`write_to_file` or `replace_file_content`), you MUST execute:
    ```powershell
    .\deploy-sync.ps1
    ```
4.  **MANDATORY VERIFICATION**: Always run `.\verify-desync.ps1` to prove the audit trail is clean.

### 🚀 Workflow
- **Repo** = The Source
- **NinjaTrader** = The Live Target (via Hard Links)
- **Deployment** = Automated via `deploy-sync.ps1`

Failure to follow this protocol will result in a code desync and compilation errors for the USER.
