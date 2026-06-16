#!/usr/bin/env python3
"""
Phase MCP Server Testing Script
Validates that all phase MCP servers are properly configured and functional

Usage:
    python scripts/test_phase_mcp_servers.py
    python scripts/test_phase_mcp_servers.py --server phase-0-hotspot
    python scripts/test_phase_mcp_servers.py --verbose
"""

import argparse
import json
import subprocess
import sys
from pathlib import Path
from typing import Dict, List, Any, Optional

# Add project root to path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

REPO_PATH = Path("C:/WSGTA/universal-or-strategy")
MCP_CONFIG = REPO_PATH / ".bob/mcp.json"


class MCPServerTester:
    """Tests MCP server configurations and functionality"""
    
    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.config = self._load_config()
        self.results: List[Dict[str, Any]] = []
    
    def _load_config(self) -> Dict[str, Any]:
        """Load MCP configuration"""
        if not MCP_CONFIG.exists():
            raise FileNotFoundError(f"MCP config not found: {MCP_CONFIG}")
        
        with open(MCP_CONFIG, 'r', encoding='utf-8') as f:
            return json.load(f)
    
    def _log(self, message: str, level: str = "INFO"):
        """Log message if verbose"""
        if self.verbose or level == "ERROR":
            prefix = {
                "INFO": "[INFO]",
                "SUCCESS": "[PASS]",
                "ERROR": "[FAIL]",
                "WARNING": "[WARN]"
            }.get(level, "[*]")
            print(f"{prefix} {message}")
    
    def test_server_config(self, server_name: str) -> Dict[str, Any]:
        """Test server configuration"""
        result = {
            "server": server_name,
            "tests": {},
            "overall": "PASS"
        }
        
        self._log(f"\nTesting server: {server_name}")
        
        servers = self.config.get("mcpServers", {})
        if server_name not in servers:
            result["tests"]["config_exists"] = {
                "status": "FAIL",
                "error": "Server not found in config"
            }
            result["overall"] = "FAIL"
            self._log(f"Server {server_name} not found in config", "ERROR")
            return result
        
        result["tests"]["config_exists"] = {"status": "PASS"}
        self._log("Config exists", "SUCCESS")
        
        server_config = servers[server_name]
        
        # Test 1: Required fields
        required_fields = ["type", "command", "args"]
        for field in required_fields:
            if field not in server_config:
                result["tests"][f"has_{field}"] = {
                    "status": "FAIL",
                    "error": f"Missing required field: {field}"
                }
                result["overall"] = "FAIL"
                self._log(f"Missing field: {field}", "ERROR")
            else:
                result["tests"][f"has_{field}"] = {"status": "PASS"}
                self._log(f"Has {field}: {server_config[field]}")
        
        # Test 2: Script file exists
        if "args" in server_config and server_config["args"]:
            script_path = Path(server_config["args"][0])
            if script_path.exists():
                result["tests"]["script_exists"] = {"status": "PASS"}
                self._log(f"Script exists: {script_path}", "SUCCESS")
            else:
                result["tests"]["script_exists"] = {
                    "status": "FAIL",
                    "error": f"Script not found: {script_path}"
                }
                result["overall"] = "FAIL"
                self._log(f"Script not found: {script_path}", "ERROR")
        
        # Test 3: Python executable
        if server_config.get("command") == "python":
            try:
                proc = subprocess.run(
                    ["python", "--version"],
                    capture_output=True,
                    text=True,
                    check=True
                )
                result["tests"]["python_available"] = {
                    "status": "PASS",
                    "version": proc.stdout.strip()
                }
                self._log(f"Python available: {proc.stdout.strip()}", "SUCCESS")
            except Exception as e:
                result["tests"]["python_available"] = {
                    "status": "FAIL",
                    "error": str(e)
                }
                result["overall"] = "FAIL"
                self._log(f"Python not available: {e}", "ERROR")
        
        # Test 4: alwaysAllow tools
        if "alwaysAllow" in server_config:
            tools = server_config["alwaysAllow"]
            result["tests"]["has_tools"] = {
                "status": "PASS",
                "count": len(tools),
                "tools": tools
            }
            self._log(f"Tools configured: {len(tools)}")
        else:
            result["tests"]["has_tools"] = {
                "status": "WARNING",
                "message": "No alwaysAllow tools configured"
            }
            self._log("No alwaysAllow tools configured", "WARNING")
        
        # Test 5: Environment variables
        if "env" in server_config:
            env = server_config["env"]
            result["tests"]["has_env"] = {
                "status": "PASS",
                "env": env
            }
            self._log(f"Environment: {env}")
            
            # Verify PYTHONPATH
            if "PYTHONPATH" in env:
                pythonpath = Path(env["PYTHONPATH"])
                if pythonpath.exists():
                    result["tests"]["pythonpath_valid"] = {"status": "PASS"}
                    self._log(f"PYTHONPATH valid: {pythonpath}", "SUCCESS")
                else:
                    result["tests"]["pythonpath_valid"] = {
                        "status": "FAIL",
                        "error": f"PYTHONPATH not found: {pythonpath}"
                    }
                    result["overall"] = "FAIL"
                    self._log(f"PYTHONPATH not found: {pythonpath}", "ERROR")
        
        return result
    
    def test_script_syntax(self, script_path: Path) -> Dict[str, Any]:
        """Test Python script syntax"""
        result = {"status": "PASS"}
        
        try:
            # Compile the script to check for syntax errors
            with open(script_path, 'r', encoding='utf-8') as f:
                code = f.read()
            
            compile(code, str(script_path), 'exec')
            self._log(f"Syntax valid: {script_path.name}", "SUCCESS")
        except SyntaxError as e:
            result = {
                "status": "FAIL",
                "error": f"Syntax error: {e}",
                "line": e.lineno
            }
            self._log(f"Syntax error in {script_path.name}: {e}", "ERROR")
        except Exception as e:
            result = {
                "status": "FAIL",
                "error": str(e)
            }
            self._log(f"Error checking {script_path.name}: {e}", "ERROR")
        
        return result
    
    def test_all_phase_servers(self) -> Dict[str, Any]:
        """Test all phase MCP servers"""
        phase_servers = [
            "phase-0-hotspot",
            "phase-1-scope",
            "phase-1-5-boundary",
            "phase-2-architecture",
            "phase-3-audit",
            "phase-4-tickets",
            "phase-5-execute",
            "phase-5-verify",
            "phase-6-review"
        ]
        
        print("\n" + "="*60)
        print("Phase MCP Server Test Suite")
        print("="*60)
        
        results = {
            "timestamp": subprocess.run(
                ["powershell", "-Command", "Get-Date -Format 'o'"],
                capture_output=True,
                text=True
            ).stdout.strip(),
            "servers_tested": len(phase_servers),
            "servers": {}
        }
        
        passed = 0
        failed = 0
        
        for server_name in phase_servers:
            result = self.test_server_config(server_name)
            results["servers"][server_name] = result
            
            if result["overall"] == "PASS":
                passed += 1
            else:
                failed += 1
            
            # Test script syntax if available
            servers = self.config.get("mcpServers", {})
            if server_name in servers:
                server_config = servers[server_name]
                if "args" in server_config and server_config["args"]:
                    script_path = Path(server_config["args"][0])
                    if script_path.exists():
                        syntax_result = self.test_script_syntax(script_path)
                        result["tests"]["syntax"] = syntax_result
                        if syntax_result["status"] == "FAIL":
                            result["overall"] = "FAIL"
                            if result["overall"] == "PASS":
                                passed -= 1
                                failed += 1
        
        results["summary"] = {
            "passed": passed,
            "failed": failed,
            "success_rate": f"{(passed/len(phase_servers)*100):.1f}%"
        }
        
        print("\n" + "="*60)
        print("Test Summary")
        print("="*60)
        print(f"Total Servers: {len(phase_servers)}")
        print(f"[PASS] Passed: {passed}")
        print(f"[FAIL] Failed: {failed}")
        print(f"Success Rate: {results['summary']['success_rate']}")
        print("="*60 + "\n")
        
        return results
    
    def generate_report(self, results: Dict[str, Any], output_file: Optional[Path] = None):
        """Generate detailed test report"""
        if output_file:
            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(results, f, indent=2)
            print(f"Report saved to: {output_file}")
        else:
            print(json.dumps(results, indent=2))


def main():
    parser = argparse.ArgumentParser(description="Test Phase MCP servers")
    parser.add_argument("--server", help="Test specific server")
    parser.add_argument("--verbose", "-v", action="store_true", help="Verbose output")
    parser.add_argument("--output", "-o", help="Output report file")
    
    args = parser.parse_args()
    
    tester = MCPServerTester(verbose=args.verbose)
    
    if args.server:
        result = tester.test_server_config(args.server)
        print(json.dumps(result, indent=2))
    else:
        results = tester.test_all_phase_servers()
        if args.output:
            tester.generate_report(results, Path(args.output))
        elif args.verbose:
            tester.generate_report(results)


if __name__ == "__main__":
    main()

# Made with Bob
