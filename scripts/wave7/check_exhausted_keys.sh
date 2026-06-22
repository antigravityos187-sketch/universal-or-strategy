#!/bin/bash

# Check which API keys are assigned to the 18 failing epics

echo "Checking API key assignments for failing epics:"
echo "================================================"
echo ""

FAILING_EPICS=(008 018 038 053 068 069 083 090 098 099 108 113 121 128 141 143 153 158)

for epic in "${FAILING_EPICS[@]}"; do
    script_file="scripts/wave7/_p0_${epic}.sh"
    if [ -f "$script_file" ]; then
        # Extract key name from BOBSHELL_API_KEY (format: bob_prod_KEYNAME_...)
        key=$(grep 'BOBSHELL_API_KEY=' "$script_file" | head -1 | cut -d"'" -f2 | cut -d'_' -f3)
        echo "EPIC-W7-${epic}: $key"
    else
        echo "EPIC-W7-${epic}: SCRIPT NOT FOUND"
    fi
done

echo ""
echo "Unique exhausted keys:"
echo "====================="
for epic in "${FAILING_EPICS[@]}"; do
    script_file="scripts/wave7/_p0_${epic}.sh"
    if [ -f "$script_file" ]; then
        grep 'BOBSHELL_API_KEY=' "$script_file" | head -1 | cut -d"'" -f2 | cut -d'_' -f3
    fi
done | sort -u

echo ""
echo "Count by key:"
echo "============="
for epic in "${FAILING_EPICS[@]}"; do
    script_file="scripts/wave7/_p0_${epic}.sh"
    if [ -f "$script_file" ]; then
        grep 'BOBSHELL_API_KEY=' "$script_file" | head -1 | cut -d"'" -f2 | cut -d'_' -f3
    fi
done | sort | uniq -c | sort -rn

# Made with Bob
