# Development Protection Protocol (DPP)

This protocol enforces strict environment separation and integrity checks to prevent code loss, file truncation, and accidental production overwrites. 

> [!IMPORTANT]
> This protocol works in tandem with the **Concurrency Guard Protocol (CGP)**. See `.agent/protocols/concurrency_guard.md` to prevent multi-agent collisions.

## 1. Environment Separation
### Production Files
- **Definition**: Any file without a `_Dev` suffix (e.g., `UniversalORStrategyV12.cs`, `V12StandardPanel_V12_001.cs`).
- **Rule**: **READ-ONLY**. Agents must NEVER edit Production files directly.
- **Access**: Used only for reading baseline logic or creating a development copy.

### Development Files
- **Definition**: Files with the `_Dev` suffix (e.g., `UniversalORStrategyV12_Dev.cs`).
- **Initialization**: Before starting work, agents MUST:
  1. Copy the relevant Production file to a new `_Dev` file.
  2. Perform initial work only on the `_Dev` file.

## 2. Integrity Checks (anti-Truncation)
Before editing or deploying any file over 1000 lines, agents MUST:
1. **Verify Line Count**: Use `view_file_outline` or `run_command` to check total lines.
2. **Compare with Expected**: If the file size is significantly smaller than previously recorded (e.g., <50%), **ABORT IMMEDIATELY**.
3. **Token Limit Awareness (CIP)**: If the file is too large for a single read (>20k tokens), agents MUST switch to chunked reading or grep. Never propose changes based on partial "blind" reads.
4. **Log Checksum/Count**: Document the line count in the `task_boundary` summary or `implementation_plan.md`.

## 3. Refactor Safety
When decomposing large files (e.g., splitting into `partial` classes):
1. **Line Sum Parity**: The sum of lines in all new partial files must roughly equal (+/- 5%) the original file size.
2. **Mandatory Backup**: The original file must be **renamed with a `_Backup` suffix** (e.g., `UniversalORStrategyV12_Backup.cs`) until the new system is confirmed to compile and run.
3. **NO DELETION**: Never delete the original production file until explicit user approval after successful live testing.

## 4. Deployment Guardrails
The `scripts/ninja_deploy.ps1` script is the gatekeeper:
- **Enforced Suffix**: It will fail if attempting to deploy a file without the `_Dev` suffix.
- **Size Audit**: It will perform a line count check before copying.
- **Overwrite Warning**: It will warn if the target in the NinjaTrader bin folder is a protected production filename that doesn't match the dev source pattern.

## 5. Pre-Task Archiving
To ensure a persistent safety net, every major task must be preceded by a mandatory archival checkpoint:
1. **Checkpoint Backup**: Create a `.bak` copy with a descriptive name suffix (e.g., `UniversalORStrategyV12_Dev.cs.bak.TaskName`).
2. **Archive Snapshot**: Copy the current state to the `ARCHIVE_SNAPSHOTS` directory with a dated timestamp and task name (e.g., `ARCHIVE_SNAPSHOTS/UniversalORStrategyV12_Dev_CheckPoint_YYYYMMDD_TaskName.cs`).
3. **Task Linkage**: Document the existence and path of these backups in the `task_boundary` or `implementation_plan.md` before making any edits.

> [!WARNING]
> Failure to follow these rules may result in "The Red Light" (System Halt) and manual code recovery efforts.
