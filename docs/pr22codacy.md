
Code complexity
+2
Code style
+2
Compatibility
+1
Fixed
-7
Potentially new
+1
Potentially fixed
-1
Files with new issues
src

V12_002.SIMA.Shadow.cs
+3
tests

LogicTests.cs
+1
tests/V12_Performance.Tests/Shadow

ShadowPropagateStopMovesTests.cs
+4
Turn on AI-powered PR reviews
Review smarter, not harder. Codacy's AI reviewer surfaces code quality metrics, PR details, and Jira context directly in GitHub.Enable AI reviewer


Showing 7 issues
MEDIUM
Code complexity


Method MockOrder::ValidateCachedEntry has a cyclomatic complexity of 9 (limit is 8)

tests/V12_Performance.Tests/Shadow/
ShadowPropagateStopMovesTests.cs

417
        private bool ValidateCachedEntry(
MEDIUM
Code complexity


Method V12_002::ValidateCachedEntry has a cyclomatic complexity of 9 (limit is 8)

src/
V12_002.SIMA.Shadow.cs

154
        internal bool ValidateCachedEntry(
HIGH
Performance


Private classes which are not derived in the current assembly should be marked as 'sealed'.

tests/V12_Performance.Tests/Shadow/
ShadowPropagateStopMovesTests.cs

14
        private class MockPositionInfo
MEDIUM
Compatibility


Provide a 'CLSCompliant' attribute for assembly 'srcassembly.dll'.

tests/V12_Performance.Tests/Shadow/
ShadowPropagateStopMovesTests.cs

1
using System;
MINOR
Code style


Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Shadow.cs

43
                if (
MINOR
Code style


Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Shadow.cs

39
                if (!ValidateLeaderPosition(kvp.Value, kvp.Key, stopOrders, out leaderStop))
HIGH
Performance


Private classes which are not derived in the current assembly should be marked as 'sealed'.

tests/V12_Performance.Tests/Shadow/
ShadowPropagateStopMovesTests.cs

22
        private class MockOrder