#!/bin/bash
# Fix EPIC-108 TICKET-1: Move IsOrderCancellable method inside class definition
# Issue: Method was placed OUTSIDE class (after line 1485 closing brace)
# Fix: Move method to line ~1480 (before #endregion at line 1484)

set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
FILE="$REPO_DIR/src/V12_002.SIMA.Lifecycle.cs"

echo "=== EPIC-108 TICKET-1 Fix: Move IsOrderCancellable Inside Class ==="
echo "Started: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

# Step 1: Verify the issue exists
echo "Step 1: Verifying method placement issue..."
if grep -A 5 "^}" "$FILE" | grep -q "IsOrderCancellable"; then
    echo "✅ Confirmed: IsOrderCancellable is outside class definition"
else
    echo "⚠️ Method may already be fixed or issue is different"
    echo "Checking current state..."
    grep -n "IsOrderCancellable" "$FILE" || echo "Method not found"
    exit 1
fi

# Step 2: Create backup
echo ""
echo "Step 2: Creating backup..."
cp "$FILE" "$FILE.backup_epic108_t1"
echo "✅ Backup created: $FILE.backup_epic108_t1"

# Step 3: Extract the method (lines 1487-1502)
echo ""
echo "Step 3: Extracting IsOrderCancellable method..."
METHOD_CONTENT=$(sed -n '1487,1502p' "$FILE")
echo "✅ Method extracted (16 lines)"

# Step 4: Remove method from wrong location (after class closing brace)
echo ""
echo "Step 4: Removing method from wrong location..."
sed -i '1487,1502d' "$FILE"
echo "✅ Method removed from line 1487-1502"

# Step 5: Insert method at correct location (before #endregion at line 1484)
echo ""
echo "Step 5: Inserting method at correct location (before line 1484)..."
# Insert before line 1484 (#endregion)
sed -i "1483 a\\
$METHOD_CONTENT" "$FILE"
echo "✅ Method inserted before #endregion"

# Step 6: Verify fix
echo ""
echo "Step 6: Verifying fix..."
if grep -B 5 "private bool IsOrderCancellable" "$FILE" | grep -q "ShouldProtectBracketOrder"; then
    echo "✅ Method is now inside class (after ShouldProtectBracketOrder)"
else
    echo "❌ Verification failed - method placement unclear"
    echo "Restoring backup..."
    mv "$FILE.backup_epic108_t1" "$FILE"
    exit 1
fi

# Step 7: Check compilation
echo ""
echo "Step 7: Checking compilation..."
cd "$REPO_DIR"
if dotnet build --no-restore > /tmp/epic108_build.log 2>&1; then
    echo "✅ Build successful"
else
    echo "❌ Build failed - restoring backup"
    cat /tmp/epic108_build.log | tail -20
    mv "$FILE.backup_epic108_t1" "$FILE"
    exit 1
fi

# Step 8: Re-run validation
echo ""
echo "Step 8: Re-running TICKET-1 validation..."
cd "$REPO_DIR"

# Use Bob CLI to re-validate (validation only, no re-execution)
bob --yolo --chat-mode advanced "You are performing RE-VALIDATION for TICKET-1 of EPIC-CCN-108 after fixing method placement.

**Context**: Previous validation failed because IsOrderCancellable was placed OUTSIDE the class definition. The method has now been moved INSIDE the class (before #endregion at line 1484).

**Task**: Re-validate TICKET-1 implementation.

**Steps**:
1) Verify method is now inside class definition
2) Verify call site replacement is correct
3) Run compilation check
4) Check CCN reduction
5) Provide PASS/FAIL verdict

**Output**: Update docs/brain/EPIC-CCN-108/ticket-1-verification.md with new verdict

**MANDATORY**: If PASS, continue with TICKET-2 execution automatically." > /tmp/epic108_t1_revalidation.log 2>&1

echo "✅ Re-validation complete"
echo ""
echo "=== Fix Complete ==="
echo "Completed: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""
echo "Check logs:"
echo "  - Build: /tmp/epic108_build.log"
echo "  - Validation: /tmp/epic108_t1_revalidation.log"
echo "  - Verification: docs/brain/EPIC-CCN-108/ticket-1-verification.md"

# Made with Bob
