#!/bin/bash
# Wave 7 Phase 1.5 Recovery Script
# Re-launches 64 incomplete epics after VM shutdown

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "=== Wave 7 Phase 1.5 Recovery Launch ==="
echo "Incomplete epics: 64"
echo "Start time: $(date)"
echo ""

launched=0

# Launch EPIC-W7-004
if [ -f "_p1_5_004.sh" ]; then
    ./_p1_5_004.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_004.sh"
fi

# Launch EPIC-W7-011
if [ -f "_p1_5_011.sh" ]; then
    ./_p1_5_011.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_011.sh"
fi

# Launch EPIC-W7-012
if [ -f "_p1_5_012.sh" ]; then
    ./_p1_5_012.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_012.sh"
fi

# Launch EPIC-W7-015
if [ -f "_p1_5_015.sh" ]; then
    ./_p1_5_015.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_015.sh"
fi

# Launch EPIC-W7-020
if [ -f "_p1_5_020.sh" ]; then
    ./_p1_5_020.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_020.sh"
fi

# Launch EPIC-W7-028
if [ -f "_p1_5_028.sh" ]; then
    ./_p1_5_028.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_028.sh"
fi

# Launch EPIC-W7-036
if [ -f "_p1_5_036.sh" ]; then
    ./_p1_5_036.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_036.sh"
fi

# Launch EPIC-W7-043
if [ -f "_p1_5_043.sh" ]; then
    ./_p1_5_043.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_043.sh"
fi

# Launch EPIC-W7-044
if [ -f "_p1_5_044.sh" ]; then
    ./_p1_5_044.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_044.sh"
fi

# Launch EPIC-W7-047
if [ -f "_p1_5_047.sh" ]; then
    ./_p1_5_047.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_047.sh"
fi

# Launch EPIC-W7-052
if [ -f "_p1_5_052.sh" ]; then
    ./_p1_5_052.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_052.sh"
fi

# Launch EPIC-W7-055
if [ -f "_p1_5_055.sh" ]; then
    ./_p1_5_055.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_055.sh"
fi

# Launch EPIC-W7-059
if [ -f "_p1_5_059.sh" ]; then
    ./_p1_5_059.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_059.sh"
fi

# Launch EPIC-W7-060
if [ -f "_p1_5_060.sh" ]; then
    ./_p1_5_060.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_060.sh"
fi

# Launch EPIC-W7-063
if [ -f "_p1_5_063.sh" ]; then
    ./_p1_5_063.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_063.sh"
fi

# Launch EPIC-W7-068
if [ -f "_p1_5_068.sh" ]; then
    ./_p1_5_068.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_068.sh"
fi

# Launch EPIC-W7-071
if [ -f "_p1_5_071.sh" ]; then
    ./_p1_5_071.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_071.sh"
fi

# Launch EPIC-W7-075
if [ -f "_p1_5_075.sh" ]; then
    ./_p1_5_075.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_075.sh"
fi

# Launch EPIC-W7-076
if [ -f "_p1_5_076.sh" ]; then
    ./_p1_5_076.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_076.sh"
fi

# Launch EPIC-W7-079
if [ -f "_p1_5_079.sh" ]; then
    ./_p1_5_079.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_079.sh"
fi

# Launch EPIC-W7-084
if [ -f "_p1_5_084.sh" ]; then
    ./_p1_5_084.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_084.sh"
fi

# Launch EPIC-W7-087
if [ -f "_p1_5_087.sh" ]; then
    ./_p1_5_087.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_087.sh"
fi

# Launch EPIC-W7-091
if [ -f "_p1_5_091.sh" ]; then
    ./_p1_5_091.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_091.sh"
fi

# Launch EPIC-W7-092
if [ -f "_p1_5_092.sh" ]; then
    ./_p1_5_092.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_092.sh"
fi

# Launch EPIC-W7-095
if [ -f "_p1_5_095.sh" ]; then
    ./_p1_5_095.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_095.sh"
fi

# Launch EPIC-W7-100
if [ -f "_p1_5_100.sh" ]; then
    ./_p1_5_100.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_100.sh"
fi

# Launch EPIC-W7-103
if [ -f "_p1_5_103.sh" ]; then
    ./_p1_5_103.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_103.sh"
fi

# Launch EPIC-W7-107
if [ -f "_p1_5_107.sh" ]; then
    ./_p1_5_107.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_107.sh"
fi

# Launch EPIC-W7-108
if [ -f "_p1_5_108.sh" ]; then
    ./_p1_5_108.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_108.sh"
fi

# Launch EPIC-W7-111
if [ -f "_p1_5_111.sh" ]; then
    ./_p1_5_111.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_111.sh"
fi

# Launch EPIC-W7-116
if [ -f "_p1_5_116.sh" ]; then
    ./_p1_5_116.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_116.sh"
fi

# Launch EPIC-W7-119
if [ -f "_p1_5_119.sh" ]; then
    ./_p1_5_119.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_119.sh"
fi

# Launch EPIC-W7-123
if [ -f "_p1_5_123.sh" ]; then
    ./_p1_5_123.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_123.sh"
fi

# Launch EPIC-W7-124
if [ -f "_p1_5_124.sh" ]; then
    ./_p1_5_124.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_124.sh"
fi

# Launch EPIC-W7-127
if [ -f "_p1_5_127.sh" ]; then
    ./_p1_5_127.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_127.sh"
fi

# Launch EPIC-W7-132
if [ -f "_p1_5_132.sh" ]; then
    ./_p1_5_132.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_132.sh"
fi

# Launch EPIC-W7-134
if [ -f "_p1_5_134.sh" ]; then
    ./_p1_5_134.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_134.sh"
fi

# Launch EPIC-W7-135
if [ -f "_p1_5_135.sh" ]; then
    ./_p1_5_135.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_135.sh"
fi

# Launch EPIC-W7-136
if [ -f "_p1_5_136.sh" ]; then
    ./_p1_5_136.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_136.sh"
fi

# Launch EPIC-W7-137
if [ -f "_p1_5_137.sh" ]; then
    ./_p1_5_137.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_137.sh"
fi

# Launch EPIC-W7-138
if [ -f "_p1_5_138.sh" ]; then
    ./_p1_5_138.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_138.sh"
fi

# Launch EPIC-W7-139
if [ -f "_p1_5_139.sh" ]; then
    ./_p1_5_139.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_139.sh"
fi

# Launch EPIC-W7-140
if [ -f "_p1_5_140.sh" ]; then
    ./_p1_5_140.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_140.sh"
fi

# Launch EPIC-W7-141
if [ -f "_p1_5_141.sh" ]; then
    ./_p1_5_141.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_141.sh"
fi

# Launch EPIC-W7-142
if [ -f "_p1_5_142.sh" ]; then
    ./_p1_5_142.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_142.sh"
fi

# Launch EPIC-W7-143
if [ -f "_p1_5_143.sh" ]; then
    ./_p1_5_143.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_143.sh"
fi

# Launch EPIC-W7-144
if [ -f "_p1_5_144.sh" ]; then
    ./_p1_5_144.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_144.sh"
fi

# Launch EPIC-W7-145
if [ -f "_p1_5_145.sh" ]; then
    ./_p1_5_145.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_145.sh"
fi

# Launch EPIC-W7-146
if [ -f "_p1_5_146.sh" ]; then
    ./_p1_5_146.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_146.sh"
fi

# Launch EPIC-W7-147
if [ -f "_p1_5_147.sh" ]; then
    ./_p1_5_147.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_147.sh"
fi

# Launch EPIC-W7-148
if [ -f "_p1_5_148.sh" ]; then
    ./_p1_5_148.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_148.sh"
fi

# Launch EPIC-W7-149
if [ -f "_p1_5_149.sh" ]; then
    ./_p1_5_149.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_149.sh"
fi

# Launch EPIC-W7-150
if [ -f "_p1_5_150.sh" ]; then
    ./_p1_5_150.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_150.sh"
fi

# Launch EPIC-W7-151
if [ -f "_p1_5_151.sh" ]; then
    ./_p1_5_151.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_151.sh"
fi

# Launch EPIC-W7-152
if [ -f "_p1_5_152.sh" ]; then
    ./_p1_5_152.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_152.sh"
fi

# Launch EPIC-W7-153
if [ -f "_p1_5_153.sh" ]; then
    ./_p1_5_153.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_153.sh"
fi

# Launch EPIC-W7-154
if [ -f "_p1_5_154.sh" ]; then
    ./_p1_5_154.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_154.sh"
fi

# Launch EPIC-W7-155
if [ -f "_p1_5_155.sh" ]; then
    ./_p1_5_155.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_155.sh"
fi

# Launch EPIC-W7-156
if [ -f "_p1_5_156.sh" ]; then
    ./_p1_5_156.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_156.sh"
fi

# Launch EPIC-W7-157
if [ -f "_p1_5_157.sh" ]; then
    ./_p1_5_157.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_157.sh"
fi

# Launch EPIC-W7-158
if [ -f "_p1_5_158.sh" ]; then
    ./_p1_5_158.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_158.sh"
fi

# Launch EPIC-W7-159
if [ -f "_p1_5_159.sh" ]; then
    ./_p1_5_159.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_159.sh"
fi

# Launch EPIC-W7-160
if [ -f "_p1_5_160.sh" ]; then
    ./_p1_5_160.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_160.sh"
fi

# Launch EPIC-W7-161
if [ -f "_p1_5_161.sh" ]; then
    ./_p1_5_161.sh &
    launched=$((launched + 1))
    
    # Progress indicator every 10 launches
    if [ $((launched % 10)) -eq 0 ]; then
        echo "Progress: $launched/64 epics launched... (waiting 12s)"
    fi
    
    # MANDATORY: 12-second delay between epic launches
    sleep 12
else
    echo "⚠️ Script not found: _p1_5_161.sh"
fi

echo ""
echo "✅ Launched $launched/64 recovery epics with 12s delays"
echo "⏳ Waiting for all epics to complete..."
wait

echo ""
echo "✅ All recovery epics completed"
echo "End time: $(date)"

# Final status check
completed=$(find docs/brain/EPIC-W7-* -name "01-scope-boundary.md" 2>/dev/null | wc -l)
echo ""
echo "Final status: $completed/161 epics complete"

if [ $completed -eq 161 ]; then
    echo "🎉 WAVE 7 PHASE 1.5 COMPLETE!"
else
    remaining=$((161 - completed))
    echo "⚠️ Still incomplete: $remaining epics"
    echo "Run this script again or investigate failures"
fi
