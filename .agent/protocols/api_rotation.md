# Protocol: API Key Rotation (Low-Limit Hardening)

## 1. TRIGGER
- **Indication**: Rovo reports "401 Unauthorized", "403 Forbidden", or "Credit limit reached".
- **Usage Target**: Every ~2,000 tokens (approx. 5-10 deep research tasks).

## 2. REPLACEMENT PROCEDURE
1. **Open Master Bridge**: `C:\WSGTA\universal-or-strategy\.agent\mcp-servers\v12_master_bridge.py`
2. **Locate `SM_HEADERS`**: At the top of the file (lines 10-14).
3. **Swap Key**:
   - Replace the string after `Authorization: Bearer ` with the new key.
   - Example: `"Authorization": "Bearer sm_NEW_KEY_HERE"`
4. **Restart Rovo**: No code changes needed, just a quick `Ctrl+C` and restart of the `acli.exe rovodev` command.

## 3. OPTIMIZATION FOR 2000 TOKENS
To maximize the life of each key:
- Use **Grep-First**: Don't let Rovo read entire 10k line files. Give him line numbers.
- **Master Bridge Cache**: The bridge strictly filters requests to prevent unnecessary calls to the Brain API.
