# A-4 SIMA Toggle Stress Test
# Sends 5x OFF/ON cycles via IPC to port 5001

function SendIPC([string]$msg) {
    try {
        $c = New-Object System.Net.Sockets.TcpClient('127.0.0.1', 5001)
        $s = $c.GetStream()
        $w = New-Object System.IO.StreamWriter($s)
        $w.AutoFlush = $true
        $w.WriteLine($msg)
        Start-Sleep -Milliseconds 250
        $c.Close()
        Write-Host "SENT: $msg"
    } catch {
        Write-Host "FAILED: $msg -- $($_.Exception.Message)"
    }
}

Write-Host "=== A-4 SIMA TOGGLE STRESS TEST ==="
Write-Host "Sending 5x OFF/ON cycles to port 5001..."
Write-Host "Check NT8 Output Window for [SIMA] lines after each ON"
Write-Host ""

1..5 | ForEach-Object {
    $n = $_
    Write-Host "--- Cycle $n ---"
    SendIPC "SET_SIMA|OFF|MES MAR26"
    Start-Sleep -Milliseconds 400
    SendIPC "SET_SIMA|ON|MES MAR26"
    Start-Sleep -Milliseconds 600
}

Write-Host ""
Write-Host "=== STRESS COMPLETE - 5 cycles sent ==="
Write-Host "Expected NT8 Output: [SIMA] TOTAL ACCOUNTS DETECTED: N (same N each time)"
