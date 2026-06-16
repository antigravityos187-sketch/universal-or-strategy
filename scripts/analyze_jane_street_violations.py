import json

# Load violations
with open('jane_street_p0_violations.json', encoding='utf-16') as f:
    data = json.load(f)

print("=" * 80)
print("JANE STREET VIOLATIONS ANALYSIS")
print("=" * 80)

print(f"\nTotal Violations: {data['summary']['total']}")
print(f"  P0 (Critical): {data['summary']['P0']}")
print(f"  P1 (High): {data['summary']['P1']}")
print(f"  P2 (Medium): {data['summary']['P2']}")

print("\nBy Category:")
for cat, count in data['by_category'].items():
    print(f"  {cat}: {count}")

print("\n" + "=" * 80)
print("SAMPLE VIOLATIONS (First 10)")
print("=" * 80)

for i, v in enumerate(data['violations'][:10], 1):
    print(f"\n{i}. Rule: {v['rule_id']}")
    print(f"   Category: {v['category']}")
    print(f"   Severity: {v['severity']}")
    print(f"   File: {v['file']}:{v['line']}")
    print(f"   Message: {v['message'][:150]}")
    if len(v['message']) > 150:
        print(f"            ...")

print("\n" + "=" * 80)
print("FILES WITH MOST VIOLATIONS")
print("=" * 80)

# Count violations per file
file_counts = {}
for v in data['violations']:
    file = v['file']
    if file not in file_counts:
        file_counts[file] = 0
    file_counts[file] += 1

# Sort by count
sorted_files = sorted(file_counts.items(), key=lambda x: x[1], reverse=True)

print(f"\nTop 20 files:")
for i, (file, count) in enumerate(sorted_files[:20], 1):
    print(f"{i:2}. {file}: {count} violations")

print(f"\nTotal files with violations: {len(file_counts)}")

# Made with Bob
