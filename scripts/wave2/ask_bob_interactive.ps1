# Ask Bob Shell on VM about file persistence (interactive - you'll see output)

$apiKey = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"

$command = "bash -l -c `"export BOBSHELL_API_KEY='$apiKey'; cd /home/malhitticrypto/universal-or-strategy; bob --chat-mode ask -p 'I am running Bob Shell agents in SSH/screen sessions on a Linux VM. When agents use write_to_file tool, the files appear to be created successfully but they do not persist on disk after the session ends. The files are missing when I check with ls. The logs show Files Created and Verified but the directory is empty. What is the correct way to make files persist when Bob Shell runs in SSH/non-interactive mode? Should I use execute_command with shell redirection instead of write_to_file?'`""

& "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" compute ssh v12-test-golden-v2 --zone=us-central1-a --project=project-14c86305-3cba-493f-a73 --command=$command

# Made with Bob
