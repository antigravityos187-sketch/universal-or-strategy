#!/usr/bin/env python3
"""
Phase 4.5 MCP Server (FastMCP) - Ticket Review
Validates tickets against Jane Street standards before execution.
"""

from fastmcp import FastMCP
import os
import sys
import json
import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore
from pathlib import Path

mcp = FastMCP("Phase 4.5 Ticket Review Coordinator")

COLLECTION_NAME = 'jane_street_knowledge_base'
CREDENTIALS_PATH = 'firebase-credentials.json'

def init_firestore():
    """Initialize Firebase using local service account credentials."""
    root_dir = Path(__file__).parent.parent
    cred_path = root_dir / CREDENTIALS_PATH
    
    if not cred_path.exists():
        return None, f"Credentials not found at {cred_path}"
        
    try:
        cred = credentials.Certificate(str(cred_path))
        if not firebase_admin._apps:
            firebase_admin.initialize_app(cred)
        return firestore.client(), None
    except Exception as e:
        return None, f"Firebase initialization failed: {str(e)}"

def query_jane_street_kb(db, terms):
    """Query Jane Street KB for extraction patterns and validation rules."""
    if not db:
        return []
    
    results = []
    collection_ref = db.collection(COLLECTION_NAME)
    
    for term in terms:
        term_lower = term.lower()
        docs = collection_ref.stream()
        
        for doc in docs:
            data = doc.to_dict()
            search_text = " ".join([
                str(doc.id),
                str(data.get('title', '')),
                str(data.get('category', '')),
                " ".join(data.get('takeaways', [])),
                " ".join(data.get('patterns', []))
            ]).lower()
            
            if term_lower in search_text:
                results.append({
                    'term': term,
                    'doc_id': doc.id,
                    'title': data.get('title', 'Unknown'),
                    'category': data.get('category', 'N/A'),
                    'takeaways': data.get('takeaways', []),
                    'patterns': data.get('patterns', [])
                })
    
    return results

def validate_ticket_scope(tickets_content):
    """Validate that tickets respect single-method boundary."""
    violations = []
    
    # Check for multi-method extraction indicators
    multi_method_indicators = [
        'extract multiple methods',
        'refactor multiple functions',
        'split across methods',
        'modify multiple methods',
        'change several methods'
    ]
    
    content_lower = tickets_content.lower()
    for indicator in multi_method_indicators:
        if indicator in content_lower:
            violations.append(f"Potential scope creep: '{indicator}' detected")
    
    # Check for reasonable ticket count (>5 tickets suggests over-engineering)
    ticket_count = tickets_content.count('## TICKET-')
    if ticket_count > 5:
        violations.append(f"Excessive ticket count: {ticket_count} tickets (max recommended: 5)")
    
    return violations

def validate_complexity_targets(tickets_content):
    """Validate that complexity targets are realistic (CYC ≤ 8)."""
    violations = []
    
    # Look for complexity targets
    lines = tickets_content.split('\n')
    for i, line in enumerate(lines):
        if 'target complexity' in line.lower() or 'cyc' in line.lower():
            # Extract numbers from line
            import re
            numbers = re.findall(r'\d+', line)
            for num in numbers:
                if int(num) > 8:
                    violations.append(
                        f"Line {i+1}: Target complexity {num} exceeds Jane Street threshold (≤8)"
                    )
    
    return violations

@mcp.tool()
def execute_phase_4_5(epic_id: str) -> dict:
    """
    Execute Phase 4.5 (Ticket Review) for an epic.
    Validates tickets against Jane Street standards before execution.
    
    Args:
        epic_id: Epic ID (e.g., EPIC-CCN-116)
    
    Returns:
        Context bundle with instructions for Bob IDE
    """
    
    # Initialize Firebase
    db, error = init_firestore()
    firebase_status = "✅ Connected" if db else f"⚠️ {error}"
    
    # Query Jane Street KB for relevant patterns
    kb_terms = [
        'extraction',
        'complexity',
        'refactoring',
        'single responsibility',
        'method extraction'
    ]
    
    kb_results = query_jane_street_kb(db, kb_terms) if db else []
    
    # Prepare context bundle
    context = {
        "phase": "Phase 4.5: Ticket Review",
        "epic_id": epic_id,
        "firebase_status": firebase_status,
        "kb_results_count": len(kb_results),
        "input_files": [
            f"docs/brain/{epic_id}/04-tickets.md",
            f"docs/brain/{epic_id}/02-architecture-plan.md",
            f"docs/brain/{epic_id}/manifest.json"
        ],
        "instructions": f"""
# Phase 4.5: Ticket Review for {epic_id}

## Firebase Status
{firebase_status}

## Jane Street Knowledge Base Results
Found {len(kb_results)} relevant documents:

{chr(10).join([f"- **{r['title']}** ({r['category']}): {len(r['takeaways'])} takeaways" for r in kb_results[:5]])}

## Input Files
1. Read `docs/brain/{epic_id}/04-tickets.md` for ticket specifications
2. Read `docs/brain/{epic_id}/02-architecture-plan.md` for architecture context
3. Read `docs/brain/{epic_id}/manifest.json` for epic metadata

## Your Task
Perform a comprehensive ticket review against Jane Street standards.

### Validation Checklist

#### 1. Scope Boundary Validation (V12.23 Protocol)
- [ ] Each ticket targets SINGLE METHOD only
- [ ] No cross-method refactoring
- [ ] No "while we're here" improvements
- [ ] Clear extraction boundaries defined
- [ ] Dependencies explicitly listed

**Violations to Check**:
- Multi-method extraction
- Scope creep indicators
- Excessive ticket count (>5 tickets)

#### 2. Complexity Target Validation (Jane Street Alignment)
- [ ] Target complexity ≤ 8 (Jane Street threshold)
- [ ] Complexity reduction realistic
- [ ] Extraction strategy sound
- [ ] No over-engineering

**Violations to Check**:
- Target CYC > 8
- Unrealistic complexity claims
- Missing complexity metrics

#### 3. Jane Street Pattern Compliance
Query Firebase KB for each ticket's extraction pattern:

{chr(10).join([f"**{r['title']}**:{chr(10)}{chr(10).join([f'  - {t}' for t in r['takeaways'][:3]])}" for r in kb_results[:3]])}

- [ ] "Make illegal states unrepresentable"
- [ ] Lock-free patterns (no `lock()` blocks)
- [ ] Actor/FSM pattern usage
- [ ] ASCII-only compliance

#### 4. Risk Assessment
- [ ] Blast radius documented
- [ ] Test coverage requirements clear
- [ ] Rollback strategy defined
- [ ] Dependencies validated

### Output File
Create `docs/brain/{epic_id}/04.5-ticket-review.md` with:

```markdown
# Ticket Review Report: {epic_id}

## Review Summary
- **Reviewer**: Phase 4.5 Coordinator
- **Date**: [ISO timestamp]
- **Firebase KB**: {firebase_status}
- **KB Documents Consulted**: {len(kb_results)}

## Validation Results

### 1. Scope Boundary Validation
**Status**: [PASS | FAIL]

**Findings**:
- [List any scope violations]
- [Confirm single-method boundary]

### 2. Complexity Target Validation
**Status**: [PASS | FAIL]

**Findings**:
- [Check all targets ≤ 8]
- [Validate reduction claims]

### 3. Jane Street Pattern Compliance
**Status**: [PASS | FAIL]

**Findings**:
- [Check each pattern]
- [Reference KB documents]

### 4. Risk Assessment
**Status**: [PASS | FAIL]

**Findings**:
- [Blast radius adequate?]
- [Test coverage sufficient?]

## Decision

**APPROVED** ✅ | **REJECTED** ❌

**Rationale**:
[Explain approval/rejection decision]

## Recommendations
1. [Recommendation 1]
2. [Recommendation 2]
3. [Recommendation 3]

## Jane Street KB References
{chr(10).join([f"- **{r['title']}** ({r['doc_id']})" for r in kb_results[:5]])}

## Next Steps
- [If APPROVED: Proceed to Phase 5 execution]
- [If REJECTED: Return to Phase 4 for ticket revision]
```

### Update Manifest
Update `docs/brain/{epic_id}/manifest.json`:
```json
{{
  "phases": {{
    "phase_4_5": {{
      "status": "completed",
      "output": "04.5-ticket-review.md",
      "decision": "APPROVED|REJECTED",
      "kb_documents_consulted": {len(kb_results)}
    }}
  }}
}}
```

## Success Criteria
- ✅ All validation checks completed
- ✅ Jane Street KB consulted
- ✅ Approval/rejection decision documented
- ✅ Manifest updated
- ✅ Review report created

## Time Budget
- **Target**: 10 minutes per epic
- **Maximum**: 15 minutes

## Automated Validation Helpers

Run these checks programmatically:

```python
# Scope boundary check
violations = validate_ticket_scope(tickets_content)

# Complexity target check  
complexity_violations = validate_complexity_targets(tickets_content)
```
""",
        "output_files": [
            f"docs/brain/{epic_id}/04.5-ticket-review.md"
        ],
        "validation_helpers": {
            "scope_check": "validate_ticket_scope(tickets_content)",
            "complexity_check": "validate_complexity_targets(tickets_content)"
        },
        "jane_street_kb": kb_results
    }
    
    return {
        "status": "success",
        "message": f"Phase 4.5 context prepared for {epic_id}",
        "context": context
    }


if __name__ == "__main__":
    mcp.run()

# Made with Bob
