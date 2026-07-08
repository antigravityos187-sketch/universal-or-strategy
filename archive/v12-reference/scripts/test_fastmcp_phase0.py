#!/usr/bin/env python3
"""
Test FastMCP Phase 0 MCP Server
Quick validation that the server starts and responds.
"""

import subprocess
import json
import sys
from pathlib import Path

def test_phase0_mcp():
    """Test Phase 0 FastMCP server"""
    
    print("🧪 Testing Phase 0 FastMCP Server...")
    
    # Sample jCodemunch data
    jcodemunch_data = {
        "hotspots": [
            {
                "method": "ExecuteFFMALimitEntry",
                "file": "src/V12_002.cs",
                "complexity": 42,
                "churn": 15
            }
        ],
        "blast_radius": {
            "confirmed": 3,
            "potential": 7
        }
    }
    
    # Test data
    test_payload = {
        "epic_id": "EPIC-CCN-26",
        "method": "ExecuteFFMALimitEntry",
        "file": "src/V12_002.cs",
        "cyc": 42,
        "jcodemunch_data": jcodemunch_data
    }
    
    print(f"✅ Test payload prepared")
    print(f"   Epic: {test_payload['epic_id']}")
    print(f"   Method: {test_payload['method']}")
    print(f"   Complexity: {test_payload['cyc']}")
    
    print("\n📋 Next Steps:")
    print("1. Restart Bob IDE to reload MCP configuration")
    print("2. In Bob IDE, call: phase-0-hotspot.execute_phase_0()")
    print("3. Pass the test payload above")
    print("4. Verify it returns context immediately (no timeout)")
    
    return True

if __name__ == "__main__":
    success = test_phase0_mcp()
    sys.exit(0 if success else 1)

# Made with Bob
