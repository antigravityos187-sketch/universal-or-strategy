import sys
sys.path.append('scripts')
from epic_manifest import load_manifest, validate_dependencies
import json

try:
    manifest = load_manifest('EPIC-CCN-16')
    print('[OK] Manifest loaded')
    print(f'Phase 2 status: {manifest["phases"]["2"]["status"]}')
    
    deps_ok = validate_dependencies('EPIC-CCN-16', '2.3')
    print(f'[OK] Dependencies satisfied: {deps_ok}')
    
    print('\nPhase 2 details:')
    print(json.dumps(manifest['phases']['2'], indent=2))
    
except FileNotFoundError as e:
    print(f'[ERROR] {e}')
    sys.exit(1)
except Exception as e:
    print(f'[ERROR] {e}')
    sys.exit(1)

# Made with Bob
