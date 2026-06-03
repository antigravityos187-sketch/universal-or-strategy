# Universal OR Strategy V12: Project Command Center

Welcome, Director. This repository contains the hardened **Universal OR Strategy V12 (Modular)** codebase and its supporting infrastructure.

> [!IMPORTANT]
> **AGENT DIRECTIVE**: This repository has evolved from the legacy "Opening Range Breakout (ORB)" monolith into the modular **Universal OR Strategy V12**. The term "ORB" now refers exclusively to a *sub-mode* within the universal engine. All agents must use the **Photon Kernel** and **Morpheus Substrate** architectural patterns defined in `docs/architecture.md`.

## 🏗️ Architecture: The Dual-Plane Engine
- **V12 Photon Kernel**: Modularized, high-fidelity execution within NinjaTrader 8 using the FSM/Actor `Enqueue` model. (Targeting .NET 4.8 / C# 8.0).
- **Morpheus Substrate**: A cross-process, lock-free architecture for autonomous scaling, telemetry, and advanced broker integrations. (Targeting .NET 8.0).

## 📁 Directory Structure

| Folder     | Purpose                                                             |
| ---------- | ------------------------------------------------------------------- |
| `src/`     | **The Alpha**: Core C# strategy and modularized kernel logic.       |
| `bin/`     | Executables and binary tools (Auditors, CLI).                       |
| `docs/`    | Architecture maps, risk reports, audit logs, and Handoff Protocols. |
| `scripts/` | Automation tools for deployment, testing, and forensic audits.       |

## 🧠 Shared AI Memory (The "Joint Brain")

To prevent AI "blindspots" between platforms (Claude Code, Cursor, Codex, Gemini), always refer to the project-local memory in `docs/brain/`:

- **Roadmap**: [task.md](docs/brain/task.md) — The single source of truth for mission progress.
- **Status State**: [phase6_closeout_state.md](docs/brain/memory/phase6_closeout_state.md) — Handoff for Phase 7.
- **Current Plan**: [implementation_plan.md](docs/brain/implementation_plan.md) — Active surgical steps.
- **PR Report**: [pr_report.md](docs/brain/pr_report.md) — Pull request analysis and findings.

## 📜 Project Governance

- **Workflow DNA**: [Institutional Workflow DNA](docs/protocol/INSTITUTIONAL_WORKFLOW_DNA.md) — The "Zero-Trust" psychology for all operations.
- **Local Terminal First**: Prioritize running agents (Codex, Claude) in the local terminal. See [IDE_GUIDE.md](IDE_GUIDE.md) for setup.
- **Handoff Protocol**: [MASTER_HANDOFF_PROTOCOL.md](docs/protocol/MASTER_HANDOFF_PROTOCOL.md) — Follow this for all agent transitions.

## 🛠️ Tool Management (Mise)

This project uses [Mise](https://mise.jdx.dev) for unified tool version management. Mise replaces nvm, pyenv, rbenv, and other version managers with a single declarative configuration.

### Quick Start

```powershell
# Install Mise (Windows)
irm https://mise.jdx.dev/install.ps1 | iex

# Install all project tools
mise install

# Run complete setup
mise run setup
```

### Common Tasks

```powershell
mise run validate      # Full pre-push validation
mise run format        # Format C# code
mise run build         # Build project
mise run test          # Run tests
mise run security      # Security scans
mise run sync          # Sync NinjaTrader hard links
```

**Full Documentation**: [docs/protocol/MISE_SETUP.md](docs/protocol/MISE_SETUP.md)

## 🚀 Key Commands

### Deployment
Synchronize the repository with your NinjaTrader 8 environment:
```powershell
./deploy-sync.ps1
# OR via Mise
mise run sync
```

### Auditing
Run the executive audit scan to discover logic risks:
```powershell
./scripts/audit_scan.ps1
```

### Validation
Run pre-push validation checks:
```powershell
# Full validation (all 13 checks)
mise run validate

# Fast validation (skip slow checks)
mise run validate-fast
```

---

_Status: Build 1111.006 (Phase 6 Structural Hardening COMPLETE - Platinum Pass)_
