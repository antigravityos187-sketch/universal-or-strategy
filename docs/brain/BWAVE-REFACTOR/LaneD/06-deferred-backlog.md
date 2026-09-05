# 06-deferred-backlog.md — BWAVE-REFACTOR Lane D

> **Prior block**: BWAVE-NEXT/LaneB/06-deferred-backlog.md was not found. This is the
> first deferred-backlog file for the BWAVE-REFACTOR wave. Future lanes should APPEND
> to this file rather than overwrite it.

---

## Block: BWAVE-REFACTOR-LANE-D (commit d712e5e6)

### DW-B37-05-D4c — CopyRule_Create_Exists_WithExpectedSignature

- **File**: src/PropTraderTools/Tests/BwaveCycLaneBTests.cs
- **Class**: BwaveCycLaneBT7Tests
- **Deferred from**: BWAVE-REFACTOR Lane D Ph4a
- **Reason**: NT8 nested type CopyRule is a private nested struct inside CopyEngine; GetNestedType resolution behavior in xUnit test context requires verification before adding the structural test
- **Resolution**: Add structural test using GetNestedType("CopyRule", BindingFlags.NonPublic)?.GetMethod("Create",...) pattern; confirm it returns non-null in test runtime before asserting
- **Blocking**: No — production code unaffected; test coverage gap only
- **Priority**: Low (P3)
