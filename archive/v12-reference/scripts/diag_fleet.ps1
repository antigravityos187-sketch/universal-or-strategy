# Diagnostic: Get fleet state after SIMA toggle stress
function SendIPC([string]$msg) {
    try {
        $c = New-Object System.Net.Sockets.TcpClient('127.0.0.1', 5001)
        $s = $c.GetStream()
        $w = New-Object System.IO.StreamWriter($s)
        $w.AutoFlush = $true
        $w.WriteLine($msg)
        Start-Sleep -Milliseconds 300
        $c.Close()
        Write-Host "SENT: $msg"
    } catch {
        Write-Host "FAILED: $($_.Exception.Message)"
    }
}

Write-Host "=== DIAG_FLEET Snapshot after 5x toggle stress ==="
SendIPC "DIAG_FLEET|MES MAR26"
Start-Sleep -Milliseconds 500
Write-Host "Done. Check NT8 Output Window for DIAG lines."
