import json

with open('epic_roadmap.json', 'r') as f:
    epics = json.load(f)

print("First 10 epics:")
for epic in epics[:10]:
    print(f"{epic['epic_number']}: {epic['method']}")

print("\n\nChecking for multi-method epics (method contains '+' or 'and'):")
multi_method = [e for e in epics if '+' in e['method'] or ' and ' in e['method'].lower()]
print(f"Found {len(multi_method)} multi-method epics")
for epic in multi_method[:5]:
    print(f"{epic['epic_number']}: {epic['method']}")

# Made with Bob
