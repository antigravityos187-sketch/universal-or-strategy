import json

with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

print(f"Total epics: {len(data)}")
pending = [e for e in data if e.get('status') != 'complete']
print(f"Pending epics: {len(pending)}")
print(f"Completed epics: {len(data) - len(pending)}")

# Made with Bob
