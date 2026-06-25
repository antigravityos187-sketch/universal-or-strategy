#!/usr/bin/env python3
"""
Phase Compliance Validator for Wave 7 Autonomous Refactoring

Validates that each phase execution complies with Integration Matrix V2:
- Correct custom mode used
- Required output files exist
- Manifest updated correctly
- Lamport event logged

Usage:
    python scripts/validate_phase_compliance.py EPIC-W7-001 0
    python scripts/validate_phase_compliance.py EPIC-W7-001 1
    python scripts/validate_phase_compliance.py --all  # Validate all epics
"""

import json
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Integration Matrix V2 - Phase Requirements
PHASE_REQUIREMENTS = {
    "0": {
        "custom_mode": "v12-phase0-hotspot",
        "output_files": ["00-hotspots.md", "manifest.json"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Hotspot Analysis"
    },
    "1": {
        "custom_mode": "v12-phase1-scope",
        "output_files": ["00-scope.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Scope Definition"
    },
    "1.5": {
        "custom_mode": "v12-phase1-5-boundary",
        "output_files": ["01-scope-boundary.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Scope Boundary Validation"
    },
    "2": {
        "custom_mode": "v12-phase2-architecture",
        "output_files": ["02-architecture-plan.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking", "graphify"],
        "phase_name": "Architecture Planning"
    },
    "3": {
        "custom_mode": "v12-phase3-audit",
        "output_files": ["03-audit-report.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "DNA Audit"
    },
    "4": {
        "custom_mode": "v12-phase4-tickets",
        "output_files": ["04-tickets.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Ticket Generation"
    },
    "4.5": {
        "custom_mode": "v12-phase4-5-review",
        "output_files": ["04-5-ticket-review.md"],
        "required_mcps": ["sequential-thinking"],
        "phase_name": "Ticket Review"
    },
    "5": {
        "custom_mode": "v12-engineer",
        "output_files": ["ticket-{ticket_id}-completion.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Ticket Execution"
    },
    "5.V": {
        "custom_mode": "v12-phase5-v-verify",
        "output_files": ["ticket-{ticket_id}-verification.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Verification"
    },
    "6": {
        "custom_mode": "v12-phase6-review",
        "output_files": ["05-completion-report.md"],
        "required_mcps": ["jcodemunch", "sequential-thinking"],
        "phase_name": "Final Review"
    }
}


class PhaseValidator:
    def __init__(self, epic_id: str, phase: str, ticket_id: Optional[str] = None):
        self.epic_id = epic_id
        self.phase = phase
        self.ticket_id = ticket_id
        self.brain_dir = Path(f"docs/brain/{epic_id}")
        self.lamport_dir = Path(".lamport/wave7")
        self.errors: List[str] = []
        self.warnings: List[str] = []
        
    def validate(self) -> Tuple[bool, List[str], List[str]]:
        """Run all validation checks. Returns (success, errors, warnings)."""
        if self.phase not in PHASE_REQUIREMENTS:
            self.errors.append(f"Unknown phase: {self.phase}")
            return False, self.errors, self.warnings
            
        req = PHASE_REQUIREMENTS[self.phase]
        
        # Check 1: Output files exist
        self._check_output_files(req["output_files"])
        
        # Check 2: Manifest updated (if not Phase 0)
        if self.phase != "0":
            self._check_manifest_updated()
        
        # Check 3: Lamport event logged
        self._check_lamport_event()
        
        # Check 4: MCP usage in output files (heuristic)
        self._check_mcp_usage(req["required_mcps"])
        
        # Check 5: Custom mode mentioned in output (heuristic)
        self._check_custom_mode_mentioned(req["custom_mode"])
        
        success = len(self.errors) == 0
        return success, self.errors, self.warnings
    
    def _check_output_files(self, output_files: List[str]):
        """Check that required output files exist."""
        for filename in output_files:
            # Handle ticket-specific files
            if "{ticket_id}" in filename:
                if not self.ticket_id:
                    self.warnings.append(f"Ticket ID required for phase {self.phase}")
                    continue
                filename = filename.replace("{ticket_id}", self.ticket_id)
            
            filepath = self.brain_dir / filename
            if not filepath.exists():
                self.errors.append(f"Missing output file: {filepath}")
            else:
                # Check file is not empty
                if filepath.stat().st_size == 0:
                    self.errors.append(f"Output file is empty: {filepath}")
    
    def _check_manifest_updated(self):
        """Check that manifest.json was updated for this phase."""
        manifest_path = self.brain_dir / "manifest.json"
        if not manifest_path.exists():
            self.errors.append(f"Manifest not found: {manifest_path}")
            return
        
        try:
            with open(manifest_path) as f:
                manifest = json.load(f)
            
            # Check phase status exists
            phase_key = f"phase_{self.phase.replace('.', '_')}"
            if phase_key not in manifest.get("phases", {}):
                self.errors.append(f"Phase {self.phase} not in manifest")
            else:
                phase_data = manifest["phases"][phase_key]
                if phase_data.get("status") != "completed":
                    self.warnings.append(f"Phase {self.phase} status is not 'completed': {phase_data.get('status')}")
        except json.JSONDecodeError as e:
            self.errors.append(f"Invalid manifest JSON: {e}")
        except Exception as e:
            self.errors.append(f"Error reading manifest: {e}")
    
    def _check_lamport_event(self):
        """Check that Lamport event was logged for this phase."""
        event_log = self.lamport_dir / "event_log.jsonl"
        if not event_log.exists():
            self.warnings.append(f"Lamport event log not found: {event_log}")
            return
        
        # Search for phase completion event
        found = False
        try:
            with open(event_log) as f:
                for line in f:
                    event = json.loads(line.strip())
                    if (event.get("epic_id") == self.epic_id and 
                        event.get("phase") == self.phase and
                        event.get("event_type") == "phase_complete"):
                        found = True
                        break
        except Exception as e:
            self.warnings.append(f"Error reading Lamport log: {e}")
            return
        
        if not found:
            self.warnings.append(f"No Lamport event found for {self.epic_id} phase {self.phase}")
    
    def _check_mcp_usage(self, required_mcps: List[str]):
        """Heuristic check: Look for MCP tool names in output files."""
        req = PHASE_REQUIREMENTS[self.phase]
        output_files = req["output_files"]
        
        for filename in output_files:
            if "{ticket_id}" in filename:
                if not self.ticket_id:
                    continue
                filename = filename.replace("{ticket_id}", self.ticket_id)
            
            filepath = self.brain_dir / filename
            if not filepath.exists():
                continue
            
            try:
                content = filepath.read_text()
                
                # Check for MCP tool usage indicators
                mcp_indicators = {
                    "jcodemunch": ["search_symbols", "get_hotspots", "get_symbol_complexity", "get_blast_radius"],
                    "sequential-thinking": ["sequentialthinking", "Sequential Thinking", "step-by-step"],
                    "graphify": ["graphify", "knowledge graph", "graph.json"]
                }
                
                for mcp in required_mcps:
                    indicators = mcp_indicators.get(mcp, [])
                    found_any = any(indicator.lower() in content.lower() for indicator in indicators)
                    if not found_any:
                        self.warnings.append(f"No evidence of {mcp} MCP usage in {filename}")
            except Exception as e:
                self.warnings.append(f"Error reading {filename}: {e}")
    
    def _check_custom_mode_mentioned(self, custom_mode: str):
        """Heuristic check: Look for custom mode name in Agent Tracking section."""
        req = PHASE_REQUIREMENTS[self.phase]
        output_files = req["output_files"]
        
        for filename in output_files:
            if "{ticket_id}" in filename:
                if not self.ticket_id:
                    continue
                filename = filename.replace("{ticket_id}", self.ticket_id)
            
            filepath = self.brain_dir / filename
            if not filepath.exists():
                continue
            
            try:
                content = filepath.read_text()
                
                # Check for Agent Tracking section with custom mode
                if "Agent Tracking" in content or "agent name" in content.lower():
                    if custom_mode not in content:
                        self.warnings.append(f"Custom mode '{custom_mode}' not mentioned in {filename}")
                else:
                    self.warnings.append(f"No Agent Tracking section found in {filename}")
            except Exception as e:
                self.warnings.append(f"Error reading {filename}: {e}")


def validate_epic_phase(epic_id: str, phase: str, ticket_id: Optional[str] = None) -> bool:
    """Validate a single epic phase. Returns True if valid."""
    validator = PhaseValidator(epic_id, phase, ticket_id)
    success, errors, warnings = validator.validate()
    
    req = PHASE_REQUIREMENTS.get(phase, {})
    phase_name = req.get("phase_name", f"Phase {phase}")
    
    print(f"\n{'='*60}")
    print(f"Validating {epic_id} - {phase_name} (Phase {phase})")
    print(f"{'='*60}")
    
    if errors:
        print(f"\n❌ ERRORS ({len(errors)}):")
        for error in errors:
            print(f"  - {error}")
    
    if warnings:
        print(f"\n⚠️  WARNINGS ({len(warnings)}):")
        for warning in warnings:
            print(f"  - {warning}")
    
    if success and not warnings:
        print(f"\n✅ Phase {phase} validation PASSED")
    elif success:
        print(f"\n✅ Phase {phase} validation PASSED (with warnings)")
    else:
        print(f"\n❌ Phase {phase} validation FAILED")
    
    return success


def validate_all_epics():
    """Validate all epics in docs/brain/EPIC-W7-*."""
    brain_dir = Path("docs/brain")
    epic_dirs = sorted(brain_dir.glob("EPIC-W7-*"))
    
    if not epic_dirs:
        print("No Wave 7 epics found in docs/brain/")
        return
    
    print(f"Found {len(epic_dirs)} Wave 7 epics")
    
    total_phases = 0
    passed_phases = 0
    failed_phases = 0
    
    for epic_dir in epic_dirs:
        epic_id = epic_dir.name
        
        # Check which phases exist
        manifest_path = epic_dir / "manifest.json"
        if not manifest_path.exists():
            continue
        
        try:
            with open(manifest_path) as f:
                manifest = json.load(f)
            
            phases = manifest.get("phases", {})
            for phase_key, phase_data in phases.items():
                if phase_data.get("status") == "completed":
                    # Extract phase number from key (e.g., "phase_0" -> "0")
                    phase = phase_key.replace("phase_", "").replace("_", ".")
                    
                    total_phases += 1
                    if validate_epic_phase(epic_id, phase):
                        passed_phases += 1
                    else:
                        failed_phases += 1
        except Exception as e:
            print(f"Error processing {epic_id}: {e}")
    
    print(f"\n{'='*60}")
    print(f"VALIDATION SUMMARY")
    print(f"{'='*60}")
    print(f"Total phases validated: {total_phases}")
    print(f"✅ Passed: {passed_phases}")
    print(f"❌ Failed: {failed_phases}")
    
    if failed_phases == 0:
        print(f"\n🎉 All phases passed validation!")
    else:
        print(f"\n⚠️  {failed_phases} phases failed validation")


def main():
    if len(sys.argv) < 2:
        print("Usage:")
        print("  python scripts/validate_phase_compliance.py EPIC-W7-001 0")
        print("  python scripts/validate_phase_compliance.py EPIC-W7-001 5 --ticket 1")
        print("  python scripts/validate_phase_compliance.py --all")
        sys.exit(1)
    
    if sys.argv[1] == "--all":
        validate_all_epics()
    else:
        epic_id = sys.argv[1]
        phase = sys.argv[2]
        
        ticket_id = None
        if "--ticket" in sys.argv:
            ticket_idx = sys.argv.index("--ticket")
            if ticket_idx + 1 < len(sys.argv):
                ticket_id = sys.argv[ticket_idx + 1]
        
        success = validate_epic_phase(epic_id, phase, ticket_id)
        sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()

# Made with Bob
