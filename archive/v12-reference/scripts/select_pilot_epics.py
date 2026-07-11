#!/usr/bin/env python3
"""Select 3 pilot epics for Wave 7 testing."""
import json

# Load roadmap
with open('epic_roadmap_wave7.json', 'r') as f:
    data = json.load(f)

epics = list(data['epics'].items())

# Filter by priority
high = [e for e in epics if e[1]['priority'] == 'high']
medium = [e for e in epics if e[1]['priority'] == 'medium']
low = [e for e in epics if e[1]['priority'] == 'low']

# Select first of each
pilot_high = high[0]
pilot_medium = medium[0]
pilot_low = low[0]

print("WAVE 7 PILOT TEST EPICS")
print("=" * 80)
print(f"\n1. HIGH COMPLEXITY:")
print(f"   Epic: {pilot_high[0]}")
print(f"   Method: {pilot_high[1]['method']}")
print(f"   CYC: {pilot_high[1]['cyc_before']}")
print(f"   File: {pilot_high[1]['file']}")

print(f"\n2. MEDIUM COMPLEXITY:")
print(f"   Epic: {pilot_medium[0]}")
print(f"   Method: {pilot_medium[1]['method']}")
print(f"   CYC: {pilot_medium[1]['cyc_before']}")
print(f"   File: {pilot_medium[1]['file']}")

print(f"\n3. LOW COMPLEXITY:")
print(f"   Epic: {pilot_low[0]}")
print(f"   Method: {pilot_low[1]['method']}")
print(f"   CYC: {pilot_low[1]['cyc_before']}")
print(f"   File: {pilot_low[1]['file']}")

print("\n" + "=" * 80)
print(f"Total pilot epics: 3")
print(f"Expected cost: 3 × $7.32 = $21.96")

# Made with Bob
