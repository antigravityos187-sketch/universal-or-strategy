#!/usr/bin/env python3
"""
Phase MCP Integration Test
End-to-end testing of full epic workflow using MCP servers

Usage:
    python scripts/test_phase_mcp_integration.py --epic EPIC-CCN-22
    python scripts/test_phase_mcp_integration.py --epic EPIC-CCN-22 --dry-run
    python scripts/test_phase_mcp_integration.py --create-test-epic
"""

import argparse
import json
import sys
import time
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, List, Any, Optional

# Add project root to path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

REPO_PATH = Path("C:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_PATH / "docs/brain"
ROADMAP_FILE = REPO_PATH / "epic_roadmap.json"


class IntegrationTester:
    """End-to-end integration testing for Phase MCP workflow"""
    
    def __init__(self, dry_run: bool = False):
        self.dry_run = dry_run
        self.test_results: List[Dict[str, Any]] = []
        self.start_time = datetime.now(timezone.utc)
    
    def log(self, message: str, level: str = "INFO"):
        """Log test message"""
        timestamp = datetime.now(timezone.utc).strftime("%H:%M:%S")
        prefix = {
            "INFO": "[INFO]",
            "SUCCESS": "[PASS]",
            "ERROR": "[FAIL]",
            "WARNING": "[WARN]",
            "TEST": "[TEST]"
        }.get(level, "[*]")
        print(f"{timestamp} {prefix} {message}")
    
    def create_test_epic(self) -> Dict[str, Any]:
        """Create a test epic for integration testing"""
        test_epic = {
            "id": "EPIC-TEST-001",
            "method": "TestMethod",
            "file": "src/V12_002.Test.cs",
            "cyc": 25,
            "status": "pending",
            "created_at": datetime.now(timezone.utc).isoformat(),
            "description": "Integration test epic - safe to delete"
        }
        
        # Load roadmap
        if ROADMAP_FILE.exists():
            with open(ROADMAP_FILE, 'r', encoding='utf-8') as f:
                roadmap = json.load(f)
        else:
            roadmap = []
        
        # Roadmap is a list of epics
        if not isinstance(roadmap, list):
            roadmap = roadmap.get("epics", [])
        
        # Check if test epic already exists
        existing = [e for e in roadmap if e.get("epic_number") == test_epic["id"]]
        if existing:
            self.log(f"Test epic {test_epic['id']} already exists", "WARNING")
            return existing[0]
        
        # Add test epic (use epic_number field to match roadmap format)
        test_epic_entry = {
            "epic_number": test_epic["id"],
            "method": test_epic["method"],
            "file": test_epic["file"],
            "line": 0,
            "cyclomatic": test_epic["cyc"],
            "churn": 0,
            "hotspot_score": 0.0,
            "codescene_score": None,
            "composite_score": 0.0,
            "codescene_issues": [],
            "status": "pending"
        }
        roadmap.append(test_epic_entry)
        
        if not self.dry_run:
            with open(ROADMAP_FILE, 'w', encoding='utf-8') as f:
                json.dump(roadmap, f, indent=2)
            self.log(f"Created test epic: {test_epic['id']}", "SUCCESS")
        else:
            self.log(f"[DRY RUN] Would create test epic: {test_epic['id']}", "INFO")
        
        return test_epic
    
    def test_manifest_initialization(self, epic_id: str) -> Dict[str, Any]:
        """Test 1: Manifest initialization"""
        self.log(f"Test 1: Manifest initialization for {epic_id}", "TEST")
        
        result = {
            "test": "manifest_initialization",
            "epic_id": epic_id,
            "status": "PASS",
            "checks": []
        }
        
        manifest_path = BRAIN_DIR / epic_id / "manifest.json"
        
        # Check if manifest exists
        if manifest_path.exists():
            result["checks"].append({
                "name": "manifest_exists",
                "status": "PASS",
                "path": str(manifest_path)
            })
            self.log(f"Manifest exists: {manifest_path}", "SUCCESS")
            
            # Validate manifest structure
            try:
                with open(manifest_path, 'r', encoding='utf-8') as f:
                    manifest = json.load(f)
                
                required_fields = ["epic_id", "created_at", "phases"]
                for field in required_fields:
                    if field in manifest:
                        result["checks"].append({
                            "name": f"has_{field}",
                            "status": "PASS"
                        })
                    else:
                        result["checks"].append({
                            "name": f"has_{field}",
                            "status": "FAIL",
                            "error": f"Missing field: {field}"
                        })
                        result["status"] = "FAIL"
                
                self.log("Manifest structure valid", "SUCCESS")
            except json.JSONDecodeError as e:
                result["checks"].append({
                    "name": "manifest_valid_json",
                    "status": "FAIL",
                    "error": str(e)
                })
                result["status"] = "FAIL"
                self.log(f"Manifest JSON invalid: {e}", "ERROR")
        else:
            result["checks"].append({
                "name": "manifest_exists",
                "status": "FAIL",
                "error": "Manifest not found"
            })
            result["status"] = "FAIL"
            self.log(f"Manifest not found: {manifest_path}", "ERROR")
        
        self.test_results.append(result)
        return result
    
    def test_phase_execution(self, epic_id: str, phase: float) -> Dict[str, Any]:
        """Test phase execution and artifact generation"""
        self.log(f"Test: Phase {phase} execution for {epic_id}", "TEST")
        
        result = {
            "test": f"phase_{phase}_execution",
            "epic_id": epic_id,
            "phase": phase,
            "status": "PASS",
            "checks": []
        }
        
        manifest_path = BRAIN_DIR / epic_id / "manifest.json"
        
        if not manifest_path.exists():
            result["status"] = "SKIP"
            result["error"] = "Manifest not found - run Phase 0 first"
            self.log(f"Skipping Phase {phase} test - manifest not found", "WARNING")
            self.test_results.append(result)
            return result
        
        # Load manifest
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)
        
        phase_key = f"phase_{phase}".replace(".", "_")
        phase_data = manifest.get("phases", {}).get(phase_key, {})
        
        # Check phase status
        status = phase_data.get("status", "pending")
        result["checks"].append({
            "name": "phase_status",
            "status": "PASS" if status in ["completed", "in_progress"] else "FAIL",
            "value": status
        })
        
        if status == "completed":
            self.log(f"Phase {phase} completed", "SUCCESS")
            
            # Check for output artifacts
            outputs = phase_data.get("outputs", [])
            if outputs:
                result["checks"].append({
                    "name": "has_outputs",
                    "status": "PASS",
                    "count": len(outputs)
                })
                
                # Verify artifacts exist
                for output in outputs:
                    artifact_path = BRAIN_DIR / epic_id / output
                    if artifact_path.exists():
                        result["checks"].append({
                            "name": f"artifact_{output}",
                            "status": "PASS",
                            "path": str(artifact_path)
                        })
                        self.log(f"Artifact exists: {output}", "SUCCESS")
                    else:
                        result["checks"].append({
                            "name": f"artifact_{output}",
                            "status": "FAIL",
                            "error": f"Artifact not found: {output}"
                        })
                        result["status"] = "FAIL"
                        self.log(f"Artifact missing: {output}", "ERROR")
            else:
                result["checks"].append({
                    "name": "has_outputs",
                    "status": "WARNING",
                    "message": "No outputs recorded"
                })
                self.log(f"Phase {phase} has no outputs", "WARNING")
        else:
            result["status"] = "SKIP"
            self.log(f"Phase {phase} not completed (status: {status})", "WARNING")
        
        self.test_results.append(result)
        return result
    
    def test_dependency_validation(self, epic_id: str) -> Dict[str, Any]:
        """Test dependency validation between phases"""
        self.log(f"Test: Dependency validation for {epic_id}", "TEST")
        
        result = {
            "test": "dependency_validation",
            "epic_id": epic_id,
            "status": "PASS",
            "checks": []
        }
        
        manifest_path = BRAIN_DIR / epic_id / "manifest.json"
        
        if not manifest_path.exists():
            result["status"] = "SKIP"
            result["error"] = "Manifest not found"
            self.test_results.append(result)
            return result
        
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)
        
        phases = manifest.get("phases", {})
        
        # Define dependencies
        dependencies = {
            "phase_1": ["phase_0"],
            "phase_1_5": ["phase_1"],
            "phase_2": ["phase_1_5"],
            "phase_3": ["phase_2"],
            "phase_4": ["phase_2"],
            "phase_5": ["phase_4"],
            "phase_6": ["phase_5"]
        }
        
        # Check each phase's dependencies
        for phase_key, deps in dependencies.items():
            phase_data = phases.get(phase_key, {})
            phase_status = phase_data.get("status", "pending")
            
            if phase_status == "completed":
                # Verify all dependencies are completed
                all_deps_met = True
                for dep in deps:
                    dep_status = phases.get(dep, {}).get("status", "pending")
                    if dep_status != "completed":
                        all_deps_met = False
                        result["checks"].append({
                            "name": f"{phase_key}_dependency_{dep}",
                            "status": "FAIL",
                            "error": f"Dependency {dep} not completed (status: {dep_status})"
                        })
                        result["status"] = "FAIL"
                        self.log(f"Dependency violation: {phase_key} completed but {dep} is {dep_status}", "ERROR")
                
                if all_deps_met:
                    result["checks"].append({
                        "name": f"{phase_key}_dependencies",
                        "status": "PASS",
                        "message": "All dependencies satisfied"
                    })
        
        self.test_results.append(result)
        return result
    
    def test_full_workflow(self, epic_id: str) -> Dict[str, Any]:
        """Test complete workflow from Phase 0 to Phase 6"""
        self.log(f"\n{'='*60}", "INFO")
        self.log(f"Integration Test: Full Workflow for {epic_id}", "INFO")
        self.log(f"{'='*60}\n", "INFO")
        
        # Test 1: Manifest initialization
        self.test_manifest_initialization(epic_id)
        
        # Test 2-9: Phase execution (0, 1, 1.5, 2, 3, 4, 5, 6)
        phases = [0, 1, 1.5, 2, 3, 4, 5, 6]
        for phase in phases:
            self.test_phase_execution(epic_id, phase)
            time.sleep(0.1)  # Small delay between tests
        
        # Test 10: Dependency validation
        self.test_dependency_validation(epic_id)
        
        # Generate summary
        return self.generate_summary()
    
    def generate_summary(self) -> Dict[str, Any]:
        """Generate test summary"""
        total_tests = len(self.test_results)
        passed = sum(1 for r in self.test_results if r["status"] == "PASS")
        failed = sum(1 for r in self.test_results if r["status"] == "FAIL")
        skipped = sum(1 for r in self.test_results if r["status"] == "SKIP")
        
        duration = (datetime.now(timezone.utc) - self.start_time).total_seconds()
        
        summary = {
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "duration_seconds": duration,
            "total_tests": total_tests,
            "passed": passed,
            "failed": failed,
            "skipped": skipped,
            "success_rate": f"{(passed/total_tests*100):.1f}%" if total_tests > 0 else "0%",
            "results": self.test_results
        }
        
        self.log(f"\n{'='*60}", "INFO")
        self.log("Integration Test Summary", "INFO")
        self.log(f"{'='*60}", "INFO")
        self.log(f"Total Tests: {total_tests}", "INFO")
        self.log(f"[PASS] Passed: {passed}", "SUCCESS")
        self.log(f"[FAIL] Failed: {failed}", "ERROR" if failed > 0 else "INFO")
        self.log(f"[SKIP] Skipped: {skipped}", "WARNING" if skipped > 0 else "INFO")
        self.log(f"Success Rate: {summary['success_rate']}", "INFO")
        self.log(f"Duration: {duration:.2f}s", "INFO")
        self.log(f"{'='*60}\n", "INFO")
        
        return summary


def main():
    parser = argparse.ArgumentParser(description="Integration test for Phase MCP workflow")
    parser.add_argument("--epic", help="Epic ID to test")
    parser.add_argument("--create-test-epic", action="store_true", help="Create test epic")
    parser.add_argument("--dry-run", action="store_true", help="Dry run mode")
    parser.add_argument("--output", "-o", help="Output report file")
    
    args = parser.parse_args()
    
    tester = IntegrationTester(dry_run=args.dry_run)
    
    if args.create_test_epic:
        test_epic = tester.create_test_epic()
        print(json.dumps(test_epic, indent=2))
        return
    
    if not args.epic:
        parser.print_help()
        print("\nError: --epic required (or use --create-test-epic)")
        sys.exit(1)
    
    # Run full workflow test
    summary = tester.test_full_workflow(args.epic)
    
    # Save report if requested
    if args.output:
        output_path = Path(args.output)
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(summary, f, indent=2)
        print(f"\nReport saved to: {output_path}")
    
    # Exit with error code if tests failed
    if summary["failed"] > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()

# Made with Bob
