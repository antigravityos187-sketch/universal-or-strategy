#!/usr/bin/env python3
"""Calculate total bobcoin usage from Phase 3 logs."""

import re

# Read the file with proper encoding handling
with open('phase3_bobcoin_costs.txt', 'rb') as f:
    data = f.read().decode('utf-8', errors='ignore')

# Extract all numbers that look like costs (format: X.XX)
pattern = r'\d+\.\d+'
matches = re.findall(pattern, data)

# Convert to floats and filter out invalid values
costs = []
for match in matches:
    try:
        cost = float(match)
        if 0 <= cost <= 10:  # Reasonable range for bobcoin costs
            costs.append(cost)
    except ValueError:
        continue

# Calculate statistics
total = sum(costs)
count = len(costs)
average = total / count if count > 0 else 0

print(f"Phase 3 Bobcoin Usage Summary")
print(f"=" * 50)
print(f"Total Entries: {count}")
print(f"Total Bobcoins: {total:.2f}")
print(f"Average per Entry: {average:.2f}")
print(f"Min Cost: {min(costs):.2f}")
print(f"Max Cost: {max(costs):.2f}")

# Made with Bob
