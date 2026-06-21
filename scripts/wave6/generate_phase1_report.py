#!/usr/bin/env python3
"""Generate Wave 6 Phase 1 completion report."""

import json
import glob
from pathlib import Path
from datetime import datetime

def generate_report():
    """Generate completion report for Wave 6 Phase 1."""
    
    completed = []
    failed = []
    
    for epic_num in range(3, 81):
        epic_id = f"EPIC-CCN-{epic_num:03d}"
        manifest_path = f'/home/malhitticrypto/universal-or-strategy/docs/brain/{epic_id}/manifest.json'
        
        try:
            with open(manifest_path) as f:
                data = json.load(f)
                phase1 = data.get('phases', {}).get('1', {})
                status = phase1.get('status', 'unknown')
                
                if status == 'completed':
                    completed.append({
                        'epic_id': epic_id,
                        'output': phase1.get('outputs', {}).get('scope_definition', 'N/A')
                    })
                elif status == 'failed':
                    failed.append(epic_id)
        except Exception as e:
            print(f"Error reading {epic_id}: {e}")
    
    # Generate report
    report_path = '/home/malhitticrypto/universal-or-strategy/docs/brain/WAVE6_PHASE1_COMPLETION_REPORT.md'
    
    with open(report_path, 'w') as f:
        f.write('# Wave 6 Phase 1 Completion Report\n\n')
        f.write(f'**Generated**: {datetime.utcnow().isoformat()}Z\n\n')
        f.write('## Summary\n\n')
        f.write(f'- **Total Epics**: 78 (EPIC-CCN-003 through EPIC-CCN-080)\n')
        f.write(f'- **Completed**: {len(completed)}/78 ({len(completed)*100//78}%)\n')
        f.write(f'- **Failed**: {len(failed)}\n\n')
        
        if len(completed) == 78:
            f.write('✅ **Status**: COMPLETE\n\n')
        else:
            f.write(f'⚠️ **Status**: INCOMPLETE ({78-len(completed)} remaining)\n\n')
        
        f.write('## Phase 1 Outputs\n\n')
        f.write('All epics generated scope definition documents:\n\n')
        
        for item in completed:
            f.write(f'- **{item["epic_id"]}**: `{item["output"]}`\n')
        
        if failed:
            f.write('\n## Failed Epics\n\n')
            for epic_id in failed:
                f.write(f'- {epic_id}\n')
        
        f.write('\n## Next Phase\n\n')
        f.write('**Phase 1.5**: Scope Boundary Validation (MANDATORY gate)\n')
        f.write('- Validates extraction stays within single-method boundary\n')
        f.write('- Prevents scope creep (V12.23 Protocol)\n')
        f.write('- Uses Sequential Thinking MCP for validation\n\n')
        
        f.write('## Agent Tracking\n\n')
        f.write('- **Phase**: 1 (Scope Definition)\n')
        f.write('- **Mode**: v12-phase1-scope (Bob CLI)\n')
        f.write('- **MCP Tools**: jCodemunch, Sequential Thinking, Graphify\n')
        f.write('- **API**: davidgreen77 (160 bobcoins)\n')
        f.write('- **Target**: CodeScene CYC ≤8 (Jane Street strict)\n')
    
    print(f'\n✅ Report generated: {report_path}')

if __name__ == '__main__':
    generate_report()

# Made with Bob
