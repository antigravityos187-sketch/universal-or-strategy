import re

pattern = re.compile(r'cat\s+>\s+/tmp/\w+\.txt\s+<<\s+[\'"]?EOF\w*[\'"]?')
test = "cat > /tmp/phase1_msg_$EPIC_ID.txt << 'EOFMSG'"
print(f'Pattern: {pattern.pattern}')
print(f'Test string: {test}')
print(f'Match: {bool(pattern.search(test))}')

# Try simpler pattern
pattern2 = re.compile(r'cat\s+>\s+/tmp/')
print(f'\nSimpler pattern match: {bool(pattern2.search(test))}')

# Check what's in the actual file
with open('building-blocks/wave7/phase1_template_wave7.sh', 'r') as f:
    content = f.read()
    if 'cat >' in content:
        print('\n✓ File contains "cat >"')
        # Find the line
        for line in content.split('\n'):
            if 'cat >' in line:
                print(f'Line: {line}')
                print(f'Match: {bool(pattern.search(line))}')
                break

# Made with Bob
