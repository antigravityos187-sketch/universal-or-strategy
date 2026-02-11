# WSGTA Universal OR Strategy: Development & Hardening Protocol (V1)

This protocol governs all AI and human development on the Universal OR Strategy project. Every failure must result in a protocol hardening step to prevent recurrence.

## 1. The Hardening Rule
**Rule**: For every bug, compilation error, or logic failure encountered, the solution MUST include a permanent "hardening" update to the project's code, scripts, or documentation to ensure the same issue cannot occur again.

## 2. Infrastructure & Workspace
- **Primary Source of Truth**: `C:\WSGTA\universal-or-strategy` (Local-First).
- **Deployment Target**: `C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\Strategies`.
- **Cloud Backup**: **GitHub ONLY**. 
- **OneDrive Prohibition**: Do NOT store or sync the active workspace or NinjaTrader folders via OneDrive.

## 3. Deployment Safety (Wipe-on-Deploy)
To prevent "Ghost Logic" and duplicate definition errors, the deployment process must:
1.  **Purge**: Delete all existing files in the target NinjaTrader bin directory matching the version/convention of the being deployed (e.g., `_Dev*`).
2.  **Verify**: Ensure no duplicate partial files (e.g., `.Sima.cs`, `.Reaper.cs`) remain if moving to a monolith.

## 4. Architectural Stability
- **Monolith Requirement**: Until a formal, tested decomposition script is implemented, the strategy shall remain a single-file monolith (`UniversalORStrategyV12_Dev.cs`).
- **No Parallel Partial Edits**: AI agents are forbidden from splitting logic into partial files without explicit oversight and verification of the NT8 compiler compatibility.

## 5. Milestone Archiving (Token Efficiency)
**Rule**: Every Milestone or Checkpoint created MUST include a physical copy of the source code at that moment.
- **Location**: Store in `ARCHIVE_SNAPSHOTS/[Milestone_Name]`.
- **Exclusion**: These folders MUST be added to `.claudeignore` or `.gitignore` immediately after creation to prevent "Context Bloat" and token waste. The AI should only "know" the archive exists, not read its contents unless explicitly asked.

## 6. Live Environment Verification (The Ctrl+F5 Rule)
**Rule**: UI alignment or state persistence issues MUST be diagnosed using live NinjaTrader data.
1. **Force Refresh**: Diagnostics must begin by pressing **Ctrl + F5** (Reload All Historical Data) to ensure a clean state.
2. **Output Audit**: The Agent must never guess which "path" is executing. They must check the **NinjaTrader Output window** for specific `Print()` messages (e.g., "Hijacked Native Slot").
3. **No Guessing**: If the Output window does not show the expected diagnostic lines, the Agent must provide the User with specific code to add for tracing before proceeding.

---
> [!IMPORTANT]
> Failure to follow these protocols will result in immediate task rollback and mandatory audit.
