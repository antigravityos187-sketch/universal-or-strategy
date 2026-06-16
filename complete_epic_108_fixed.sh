#!/bin/bash
# EPIC-108 Completion Script (Fixed with full bob path)
# Revalidates T1, then executes T2-T5 sequentially

set -e
cd /home/malhitticrypto/universal-or-strategy

BOB_PATH="/home/malhitticrypto/.npm-global/bin/bob"
export BOBSHELL_API_KEY='bob_prod_bob-admin_c0a5e8f4-3b2d-4e1a-9f7c-8d6e5b4a3c2d'

echo "=== EPIC-108 Completion Script ==="
echo "Started: $(date)"
echo ""

# Step 1: Revalidate T1
echo "=== Step 1: Revalidating TICKET-1 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5v_108_t1.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-1 revalidation failed"
    exit 1
fi
echo "✅ TICKET-1 revalidated"
echo ""

# Step 2: Execute T2
echo "=== Step 2: Executing TICKET-2 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5_108_t2.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-2 execution failed"
    exit 1
fi
echo "✅ TICKET-2 executed"
echo ""

# Step 3: Validate T2
echo "=== Step 3: Validating TICKET-2 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5v_108_t2.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-2 validation failed"
    exit 1
fi
echo "✅ TICKET-2 validated"
echo ""

# Step 4: Execute T3
echo "=== Step 4: Executing TICKET-3 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5_108_t3.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-3 execution failed"
    exit 1
fi
echo "✅ TICKET-3 executed"
echo ""

# Step 5: Validate T3
echo "=== Step 5: Validating TICKET-3 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5v_108_t3.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-3 validation failed"
    exit 1
fi
echo "✅ TICKET-3 validated"
echo ""

# Step 6: Execute T4
echo "=== Step 6: Executing TICKET-4 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5_108_t4.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-4 execution failed"
    exit 1
fi
echo "✅ TICKET-4 executed"
echo ""

# Step 7: Validate T4
echo "=== Step 7: Validating TICKET-4 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5v_108_t4.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-4 validation failed"
    exit 1
fi
echo "✅ TICKET-4 validated"
echo ""

# Step 8: Execute T5
echo "=== Step 8: Executing TICKET-5 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5_108_t5.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-5 execution failed"
    exit 1
fi
echo "✅ TICKET-5 executed"
echo ""

# Step 9: Validate T5
echo "=== Step 9: Validating TICKET-5 ==="
bash /home/malhitticrypto/universal-or-strategy/_p5v_108_t5.sh
if [ $? -ne 0 ]; then
    echo "❌ TICKET-5 validation failed"
    exit 1
fi
echo "✅ TICKET-5 validated"
echo ""

echo "=== EPIC-108 COMPLETE ==="
echo "Completed: $(date)"
echo "All 5 tickets executed and validated successfully"

# Made with Bob
