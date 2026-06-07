#!/usr/bin/env python3
"""Apply CodeFactor fixes while preserving line endings."""

import re

def fix_v12_002_cs():
    """Add blank line before comment at line 25."""
    with open('src/V12_002.cs', 'rb') as f:
        content = f.read()
    
    # Convert to string for processing
    text = content.decode('utf-8')
    lines = text.split('\n')
    
    # Insert blank line before line 25 (index 24)
    if len(lines) > 24 and '// EPIC-CCN-12' in lines[24]:
        lines.insert(24, '')
    
    # Write back
    with open('src/V12_002.cs', 'wb') as f:
        f.write('\n'.join(lines).encode('utf-8'))
    print("Fixed src/V12_002.cs")

def fix_shadow_cs():
    """Fix parenthesis placement and blank lines in Shadow.cs."""
    with open('src/V12_002.SIMA.Shadow.cs', 'rb') as f:
        content = f.read()
    
    text = content.decode('utf-8')
    
    # Fix: Move closing ) to previous line and remove space
    text = re.sub(r'(\s+out Order leaderStop)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(ConcurrentDictionary<string, Order> stopOrders)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(\s+out double lastKnownPrice)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(ConcurrentDictionary<string, double> leaderLastStopPrice)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(\s+out bool waitingOnFollower)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(\|\| ctx == null)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(string dispatchId)\r?\n\s+\)', r'\1)', text)
    text = re.sub(r'(fsm\.AccountName)\r?\n\s+\)', r'\1)', text)
    
    # Add blank lines after specific closing braces
    lines = text.split('\n')
    new_lines = []
    for i, line in enumerate(lines):
        new_lines.append(line)
        # Add blank after } before foreach at line ~217
        if i < len(lines) - 1 and line.strip() == '}' and 'foreach (string followerEntryName' in lines[i+1]:
            new_lines.append('')
        # Add blank after } before foreach at line ~228
        if i < len(lines) - 1 and line.strip() == '}' and 'foreach (var kvp' in lines[i+1]:
            new_lines.append('')
        # Add blank after { before waitingOnFollower at line ~252
        if i < len(lines) - 1 and line.strip() == '{' and 'waitingOnFollower = false' in lines[i+1]:
            new_lines.append('')
    
    with open('src/V12_002.SIMA.Shadow.cs', 'wb') as f:
        f.write('\n'.join(new_lines).encode('utf-8'))
    print("Fixed src/V12_002.SIMA.Shadow.cs")

def fix_logic_tests():
    """Fix parenthesis placement in LogicTests.cs."""
    with open('tests/LogicTests.cs', 'rb') as f:
        content = f.read()
    
    text = content.decode('utf-8')
    
    # Fix: Move ) to previous line at line 129
    text = re.sub(r'("ENTRY_1\|5315\.75\|2\|1\|0\|3")\r?\n\s+\) \+', r'\1) +', text)
    
    # Fix: Move ) to previous line at line 212
    text = re.sub(r'(IReadOnlyList<StickyStateSection> right)\r?\n\s+\)', r'\1)', text)
    
    with open('tests/LogicTests.cs', 'wb') as f:
        f.write(text.encode('utf-8'))
    print("Fixed tests/LogicTests.cs")

if __name__ == '__main__':
    fix_v12_002_cs()
    fix_shadow_cs()
    fix_logic_tests()
    print("\nAll fixes applied!")

# Made with Bob
