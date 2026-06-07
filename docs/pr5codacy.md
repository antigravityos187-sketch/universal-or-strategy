MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Symmetry.BracketFSM.cs

383
            if (!ValidateFsmEventPreconditions(evt, out FollowerBracketFSM fsm))
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Orders.Management.Cleanup.cs

462
                if (!ShouldValidateOrder(order))
MEDIUM
Compatibility

Provide a 'CLSCompliant' attribute for assembly 'srcassembly.dll'.

tests/V12_Performance.Tests/Infrastructure/
LatencyProbeTests.cs

1
using System;
HIGH
Error prone

Catch a list of specific exception subtype or use exception filters instead.

src/
V12_002.Lifecycle.cs

529
            catch { }
MEDIUM
Compatibility

Provide a 'CLSCompliant' attribute for assembly 'srcassembly.dll'.

tests/V12_Performance.Tests/Infrastructure/
LogBufferThreadStaticTests.cs

1
using System;
HIGH
Security

Insecure Modules Libraries

Handle the exception or explain in a comment why it can be ignored.

src/
V12_002.Lifecycle.cs

529
            catch { }
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Orders.Management.Cleanup.cs

466
                if (!HasV12OrderPrefix(name))
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Dispatch.cs

420
                if (kvp.Value)
MEDIUM
Performance

Remove this useless assignment to local variable 'hydratedCount'.

src/
V12_002.SIMA.Lifecycle.cs

284
                            hydratedCount++;
HIGH
Error prone

Do not catch NullReferenceException; test for null instead.

src/
V12_002.Orders.Callbacks.Execution.cs

62
            catch (NullReferenceException ex)
HIGH
Error prone

Catch a list of specific exception subtype or use exception filters instead.

src/
V12_002.UI.IPC.cs

428
                catch { }
MINOR
Code style

Either remove or fill this block of code.

src/
V12_002.SIMA.Lifecycle.cs

61
                    catch { }
MINOR
Code style

Either remove or fill this block of code.

src/
V12_002.UI.IPC.cs

428
                catch { }
HIGH
Error prone

Catch a list of specific exception subtype or use exception filters instead.

src/
V12_002.SIMA.Lifecycle.cs

61
                    catch { }
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

tests/V12_Performance.Tests/Infrastructure/
LatencyProbeTests.cs

92
                if (!IsValid)
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.UI.Panel.Helpers.cs

732
                if (typeName == "ChartTab" || typeName.Contains("ChartTab"))
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Lifecycle.cs

761
                else if (
HIGH
Error prone

Use a format provider when parsing date and time.

src/
V12_002.Lifecycle.cs

68
                SessionEnd = DateTime.Parse("16:00");
HIGH
Security

Insecure Modules Libraries

Handle the exception or explain in a comment why it can be ignored.

src/
V12_002.SIMA.Lifecycle.cs

61
                    catch { }
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Lifecycle.cs

43
            if (_simaTogglePending != 0)
MEDIUM
Error prone

Introduce a new variable instead of reusing the parameter 'proxyOrders'.

src/
V12_002.SIMA.Dispatch.cs

1172
                    proxyOrders = legacyOrdersLmt;
MINOR
Code style

Add curly braces around the nested statement(s) in this 'else' block.

src/
V12_002.Orders.Callbacks.Execution.cs

437
                            else
MEDIUM
Performance

"EndsWith" overloads that take a "char" should be used

tests/
LogicTests.cs

172
                    if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2)
MEDIUM
Performance

Implement 'IEquatable<T>' in value type 'LatencyProbe'.

tests/V12_Performance.Tests/Infrastructure/
LatencyProbeTests.cs

81
    public struct LatencyProbe
MINOR
Code style

Either remove or fill this block of code.

src/
V12_002.Lifecycle.cs

529
            catch { }
HIGH
Security

Insecure Modules Libraries

Handle the exception or explain in a comment why it can be ignored.

src/
V12_002.UI.IPC.cs

428
                catch { }
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Lifecycle.cs

1153
                        if (!isV12)
HIGH
Error prone

Use a format provider when parsing date and time.

src/
V12_002.Lifecycle.cs

67
                SessionStart = DateTime.Parse("09:30");
HIGH
Error prone

Replace this use of 'Task.WaitAll' with 'await Task.WhenAll'.

tests/V12_Performance.Tests/Infrastructure/
LogBufferThreadStaticTests.cs

71
            Task.WaitAll(tasks);
MINOR
Code style

Remove the array type; it is redundant.

src/
V12_002.UI.Panel.Helpers.cs

771
                string[] fieldNames = new string[] { "chartTrader", "ChartTrader", "chartTraderControl", "_chartTrader" };
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.UI.Panel.Helpers.cs

550
                if (result == null)
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Orders.Callbacks.Execution.cs

435
                            if (activePositions.TryGetValue(entryName, out closedPos) && closedPos != null)
HIGH
Error prone

Add a 'default' clause to this 'switch' statement.

src/
V12_002.UI.IPC.cs

311
                        switch (validationResult)
MEDIUM
Code complexity

Method TargetConfig::UpdateConfigControlsEnabled has a cyclomatic complexity of 9 (limit is 8)

src/
V12_002.UI.Panel.Handlers.cs

801
        private void UpdateConfigControlsEnabled(int count)
MEDIUM
Code complexity

Method V12_002::PopulatePhotonSlot has 11 parameters (limit is 8)

src/
V12_002.SIMA.Dispatch.cs

1027
        private FleetDispatchSlot PopulatePhotonSlot(
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Lifecycle.cs

257
                if (!System.IO.Directory.Exists(logsDirInit))
MEDIUM
Code complexity

Method TargetConfig::UpdateConfigRowsVisibility has a cyclomatic complexity of 9 (limit is 8)

src/
V12_002.UI.Panel.Handlers.cs

823
        private void UpdateConfigRowsVisibility(int count)
MEDIUM
Code complexity

Method V12_002::HydrateFSMsFromWorkingOrders has a cyclomatic complexity of 65 (limit is 8)

src/
V12_002.SIMA.Lifecycle.cs

731
        private void HydrateFSMsFromWorkingOrders()
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.SIMA.Lifecycle.cs

759
                else if (entryState == OrderState.Accepted)
MINOR
Code style

Add curly braces around the nested statement(s) in this 'if' block.

src/
V12_002.Orders.Callbacks.Execution.cs

510
                            if (activePositions.TryGetValue(entryName, out trimPos) && trimPos != null)
MINOR
Code style

Remove this initialization to '_ipcHardeningRejectCount', the compiler will do that for you.

src/
V12_002.UI.IPC.cs

48
        private int _ipcHardeningRejectCount = 0;
MINOR
Code style

Add curly braces around the nested statement(s) in this 'foreach' block.

src/
V12_002.SIMA.Dispatch.cs

1044
            foreach (var st in stagedTargets)
MEDIUM
Code complexity

Method V12_002::EnqueueToPhotonRing has 59 lines of code (limit is 50)

src/
V12_002.SIMA.Dispatch.cs

1068
        private void EnqueueToPhotonRing(
MEDIUM
Code complexity

Method TargetConfig::SetT2T5ButtonsVisible has a cyclomatic complexity of 9 (limit is 8)

src/
V12_002.UI.Panel.Handlers.cs

857
        private void SetT2T5ButtonsVisible(int count)