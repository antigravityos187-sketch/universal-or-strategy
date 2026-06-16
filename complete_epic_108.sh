#!/bin/bash
# Complete EPIC-108 execution (T1 revalidation + T2-T5 execution)
# Conservative approach: Finish EPIC-108 before Phase 6

set -e

EPIC_DIR="/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-108"
LOG_DIR="/home/malhitticrypto/universal-or-strategy/logs"
SRC_FILE="/home/malhitticrypto/universal-or-strategy/src/V12_002.SIMA.Lifecycle.cs"

echo "=== EPIC-108 Completion Script ==="
echo "Started: $(date)"
echo ""

# Step 1: Verify method placement
echo "Step 1: Verifying IsOrderCancellable method placement..."
METHOD_LINE=$(grep -n "private bool IsOrderCancellable" "$SRC_FILE" | cut -d: -f1)
CLASS_END=$(grep -n "^}" "$SRC_FILE" | tail -1 | cut -d: -f1)

echo "  Method at line: $METHOD_LINE"
echo "  Class ends at line: $CLASS_END"

if [ "$METHOD_LINE" -lt "$CLASS_END" ]; then
    echo "  ✅ Method IS inside class (correct placement)"
    REVALIDATE=true
else
    echo "  ❌ Method is outside class (needs fix)"
    REVALIDATE=false
fi

# Step 2: Revalidate T1 if placement is correct
if [ "$REVALIDATE" = true ]; then
    echo ""
    echo "Step 2: Re-running T1 validation..."
    cd /home/malhitticrypto/universal-or-strategy
    bash _p5v_108_t1.sh > "$LOG_DIR/epic_108_t1_revalidation.log" 2>&1
    
    # Check validation result
    if grep -q "PASS" "$EPIC_DIR/ticket-1-verification.md"; then
        echo "  ✅ T1 validation PASSED"
        T1_STATUS="PASS"
    else
        echo "  ❌ T1 validation FAILED"
        T1_STATUS="FAIL"
        cat "$LOG_DIR/epic_108_t1_revalidation.log"
        exit 1
    fi
else
    echo ""
    echo "Step 2: SKIPPED (method placement incorrect)"
    exit 1
fi

# Step 3: Execute T2-T5 sequentially
echo ""
echo "Step 3: Executing remaining tickets (T2-T5)..."

for TICKET in 2 3 4 5; do
    echo ""
    echo "=== Processing TICKET-$TICKET ==="
    
    # Execute ticket
    echo "  Executing..."
    bash "_p5_108_t${TICKET}.sh" > "$LOG_DIR/phase5/epic_108_t${TICKET}_execution.log" 2>&1
    
    # Validate ticket
    echo "  Validating..."
    bash "_p5v_108_t${TICKET}.sh" > "$LOG_DIR/phase5v/epic_108_t${TICKET}_validation.log" 2>&1
    
    # Check result
    if grep -q "PASS" "$EPIC_DIR/ticket-${TICKET}-verification.md"; then
        echo "  ✅ TICKET-$TICKET passed validation"
    else
        echo "  ❌ TICKET-$TICKET failed validation"
        echo "  Check: $EPIC_DIR/ticket-${TICKET}-verification.md"
        echo "  Log: $LOG_DIR/phase5v/epic_108_t${TICKET}_validation.log"
        exit 1
    fi
done

# Step 4: Final status
echo ""
echo "=== EPIC-108 Completion Summary ==="
echo "Completed: $(date)"
echo ""
echo "Status:"
echo "  ✅ TICKET-1: Revalidated and passed"
echo "  ✅ TICKET-2: Executed and validated"
echo "  ✅ TICKET-3: Executed and validated"
echo "  ✅ TICKET-4: Executed and validated"
echo "  ✅ TICKET-5: Executed and validated"
echo ""
echo "✅ EPIC-CCN-108 Phase 5 COMPLETE"
echo ""
echo "Next step: Launch Phase 6 for all 7 epics"

# Made with Bob
