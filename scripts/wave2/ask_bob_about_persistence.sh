#!/bin/bash
# Ask Bob Shell on VM about file persistence issue
# This runs in the foreground so you can see the output

gcloud compute ssh v12-test-golden-v2 \
  --zone=us-central1-a \
  --project=project-14c86305-3cba-493f-a73 \
  --command="cd /home/malhitticrypto/universal-or-strategy && bob --chat-mode ask -p 'I am running Bob Shell agents in SSH/screen sessions on a Linux VM. When agents use write_to_file tool, the files appear to be created successfully (no errors reported), but they do not persist on disk after the session ends. The files are missing when I check with ls. The logs show Files Created and Verified but the directory is empty (total 0). What is the correct way to make files persist when Bob Shell runs in SSH/non-interactive mode? Should I use execute_command with shell redirection instead of write_to_file?'"

# Made with Bob
