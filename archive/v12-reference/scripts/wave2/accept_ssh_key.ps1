# Accept SSH key for GCP VM
# This script auto-accepts the SSH host key by piping 'y' to gcloud ssh

$command = "echo connected"
$process = Start-Process -FilePath "gcloud" -ArgumentList "compute","ssh","v12-test-golden-v2","--zone=us-central1-a","--command=`"$command`"" -NoNewWindow -Wait -PassThru -RedirectStandardInput "y"

Write-Host "Exit code: $($process.ExitCode)"

# Made with Bob
