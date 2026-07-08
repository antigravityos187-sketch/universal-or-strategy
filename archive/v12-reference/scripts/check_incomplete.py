import json

with open('epic_roadmap.json') as f:
    data = json.load(f)

incomplete = [e for e in data if e.get('status') != 'COMPLETE']
print(f"Incomplete epics: {len(incomplete)}/{len(data)}")
print("\nFirst 10 incomplete:")
for e in incomplete[:10]:
    print(f"  {e['epic_number']}: {e['method']} (status: {e.get('status', 'PENDING')})")

# Made with Bob
