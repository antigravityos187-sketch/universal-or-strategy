#!/bin/bash
# Debug: test Bob write_to_file using the VM's pre-baked BOBSHELL_API_KEY via bash -l
# bash -l sources ~/.profile which exports the valid key
cd /home/malhitticrypto/universal-or-strategy
bash -l -c "
  cd /home/malhitticrypto/universal-or-strategy
  bob --accept-license --max-coins 10 \
    -p 'Use write_to_file to create a file called test.txt with content: Hello World. Then use read_file to verify it exists and print its contents. Report success or failure.' \
    > /tmp/bob_write_test.log 2>&1
  echo DONE_EXIT=\$? >> /tmp/bob_write_test.log
"
echo "=== BOB LOG ==="
cat /tmp/bob_write_test.log
echo "=== FILE CHECK ==="
ls -lh /home/malhitticrypto/universal-or-strategy/test.txt 2>/dev/null || echo "test.txt NOT FOUND"
cat /home/malhitticrypto/universal-or-strategy/test.txt 2>/dev/null || true
