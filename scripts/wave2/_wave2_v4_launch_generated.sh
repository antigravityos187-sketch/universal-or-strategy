#!/bin/bash
# V12 Wave 2 v4 Orchestrator - Safe Budget with Tracking
# Generated: 2026-06-12T19:14:54.370144+00:00
# 9 agents, 10 API keys (1:1 mapping)
# Budget: 150 bobcoins/epic × 9 = 1350 bobcoins
# Available: 1600 bobcoins (10 APIs × 160)
# Safety Margin: 250 bobcoins (15.6%)

# Global git identity
git config --global user.email "malhitticrypto@gmail.com"
git config --global user.name "malhitticrypto"

mkdir -p /home/malhitticrypto/universal-or-strategy/logs

# Pull latest repo
cd /home/malhitticrypto/universal-or-strategy && git pull --ff-only origin main || true

echo '[WAVE2-V4] Launching parallel Bob Shell agents (SAFE BUDGET)...'
echo '[WAVE2-V4] Budget: 150 bobcoins per epic'
echo '[WAVE2-V4] Total: 1350 / 1600 bobcoins (15.6% safety margin)'

screen -dmS v12-EPIC-CCN-107 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-107: Extract ProcessIpcCommands (complexity 76 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-107.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-107.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-107 (ProcessIpcCommands, CYC 76) with b (2).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-108 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-108: Extract ProcessOnExecutionUpdate (complexity 67 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-108.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-108.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-108 (ProcessOnExecutionUpdate, CYC 67) with b.json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-109 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-109: Extract HydrateFSMsFromWorkingOrders (complexity 45 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-109.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-109.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-109 (HydrateFSMsFromWorkingOrders, CYC 45) with bob (1).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-110 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-110: Extract HandleFlatPositionUpdate (complexity 37 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-110.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-110.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-110 (HandleFlatPositionUpdate, CYC 37) with bob (2).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-111 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-111: Extract AdoptFleetOrders (complexity 37 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-111.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-111.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-111 (AdoptFleetOrders, CYC 37) with bob (3).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-112 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-112: Extract ExtractTargetConfiguration (complexity 31 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-112.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-112.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-112 (ExtractTargetConfiguration, CYC 31) with bob (4).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-113 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-113: Extract SweepBrokerOrders (complexity 28 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-113.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-113.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-113 (SweepBrokerOrders, CYC 28) with bob (5).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-114 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_65hPWuoJAPhLQKgnKSePPDiqS5YRKW1XDF1LM8kRporvu9XTpgAaY4WYvJgAe72VzRDARKEQzqzMei9UqCj28buk_2Astcnxpem897Pn91xpJXnKY6N7dMhDXAriwNtncfzsB' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-114: Extract FlattenSinglePosition (complexity 27 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-114.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-114.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-114 (FlattenSinglePosition, CYC 27) with bob (6).json (150 bobcoins)'
sleep 1
screen -dmS v12-EPIC-CCN-115 bash -l -c "export BOBSHELL_API_KEY='bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq' && cd /home/malhitticrypto/universal-or-strategy && bob --accept-license --chat-mode plan --max-coins 150 -p 'Execute complete epic-intake workflow for EPIC-CCN-115: Extract ExecuteRetestEntry (complexity 26 -> 8). Run all phases: hotspot analysis, scope definition, scope boundary validation, architecture planning, DNA audit, and ticket generation.' > /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-115.log 2>&1; echo DONE_EXIT=$? >> /home/malhitticrypto/universal-or-strategy/logs/EPIC-CCN-115.log"
echo '[WAVE2-V4] Launched: EPIC-CCN-115 (ExecuteRetestEntry, CYC 26) with bob.json (150 bobcoins)'
sleep 1

sleep 2
echo '[WAVE2-V4] All 9 agents launched (SAFE BUDGET MODE).'
echo '[WAVE2-V4] Each agent: dedicated API + 150 bobcoins.'
echo '[WAVE2-V4] Reserve: 250 bobcoins (15.6% safety margin).'
screen -ls || true

# API Allocation Summary
# EPIC-CCN-107 → API #1 (b (2).json)
# EPIC-CCN-108 → API #2 (b.json)
# EPIC-CCN-109 → API #3 (bob (1).json)
# EPIC-CCN-110 → API #4 (bob (2).json)
# EPIC-CCN-111 → API #5 (bob (3).json)
# EPIC-CCN-112 → API #6 (bob (4).json)
# EPIC-CCN-113 → API #7 (bob (5).json)
# EPIC-CCN-114 → API #8 (bob (6).json)
# EPIC-CCN-115 → API #9 (bob.json)

echo '[WAVE2-V4] Done. Logs: /home/malhitticrypto/universal-or-strategy/logs'
echo '[WAVE2-V4] Monitor bobcoin usage to prevent negatives!'