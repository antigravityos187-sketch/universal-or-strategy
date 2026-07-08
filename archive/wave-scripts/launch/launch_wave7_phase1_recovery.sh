#!/bin/bash
# Wave 7 Phase 1 Recovery Launch Script
# Executes 56 epics: 43 never-launched + 13 failed
# Uses 15 Bob Shell API keys with rotation

set -e

# Bob Shell API Keys (15 keys from docs/API/)
API_KEYS=(
  "bob_prod_bob-admin_3SKCJRKM5kTYHCCbLL5KcebgMYk1ZP2CuW4pmQkVPfhzcTqDMiRQAiauiZ2RDMAZ3RY2MihHAJkzzSPJsYvK7vjW_3cM1RH5zLE5owNwVMg2Gnc9ScdPKyNnWmnJy6nXXJH6i"
  "bob_prod_bob-admin_58sbZY3cBGWbej6dmAvwYeRTKyuceZJppgm4vYoS7bb2yzKqFxwAmzsR46D6G86LVJWNBmsUaZLBpMgRpiZyPQDf_GpwXFSKRi7nWHCJP2m1S6guZ1Y4kzUKBR9C1mrkKQm3s"
  "bob_prod_bob-admin_2aNT7CZ3HecXSysJasUajLjgd5mGiXmmmLMxAk3ARKayj6hzL53KRLBTSZYvVZyptBa8ydotXWvezrgdru7v8TfW_DjcCaV1W6wMZkjQ2sjZrN8m8Hnw9GS5iVFkGqb34eKEJ"
  "bob_prod_bob-admin_5PbEKjG9uqQAMCizgY3pPmDxRFiTW6XJvBvp3RDcYGbT6vk5tE8FJmLUCWsT1WfH6nn7WYoxMHrXe3GfhFo8LPgi_5dYLFXNSc2SBcHL9ZrQXwxj1XrvbotdUsWJXVTv13u2a"
  "bob_prod_bob-admin_aRSjzM4xwaEhbcjDdViPqh3giwmvtQksbGerdHvRxq8MPyN2X7KHUU9q6H9DYDBj2YaJwhkgDci2HcT1gRbS9d6_9MHxQ1wMuJVJYeJG2gbRe4NCDCAdf2GBd4wKLhQMg1hS"
  "bob_prod_bob-admin_22HihoispYfg9TBX2smEUD6b18c9zwRRFnssCBDwTquLYiH4bvHLkntgiNVgt5DZtcfSUqE7LDbMBJxrb9W6cCQc_AKDtJW7uTVi1ZpoDxktCmz2WNcvi2REwiTe88STW7h4J"
  "bob_prod_bob-admin_4tdFq99zrsvGGgqpLmsaDid9QqycnQT74EtvTFttZpWcJdWW5L3VEQuCTsQxM1GTWDCd8HWkPW9jcWPFqYp5hW9v_8TSHVEQRkt3DbE6zuqMQHoajMzLtuUUYdUxTxSrofQMg"
  "bob_prod_bob-admin_2FTTdxZo3mEs7ek4rbpBLVdpkTinfTdgG6Zj2CK9D2A7ct7TwUi1CyQSaHwqEozi9npR6Go4BLkBzAyxQzaWpaii_B1y7Ji37WbeKFZgREwNCqjQEJCdzqfhwpCN9Rfa1BiMN"
  "bob_prod_bob-admin_3h6asj27KMtNwpBXWi1m9aL7rJa7ZQqPwCTBE4kSTa7V9ZZQZXw9Fot4umQDChd476i5wu7z4njG2ggdJrUQQ6uF_9axjeHVDZUBdsKxMWJm5ZwiNuGwsHz6TpjbaF8VeX8tk"
  "bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ"
  "bob_prod_bob-admin_hXrAgTSbvVHGpmUxqohSv8SirgVeXpEBSoF1wyb8xBQz2PBMmgyKfozT2kJP5RuCbqBVsW91Z6pp8auuGBjb53P_2qJE8WD7whnc8nYcahv4AevV8ekHvP1K4zbYggeoSvZ2"
  "bob_prod_bob-admin_2SC4VtHL6svY4W1mLFsfLCztVEc6wzuuoXFevyjQCEEojgcH7LLtWUBQiwZ9Q2q6cy8Hmrurpm1KkpXqaoSiKEez_5Vu6PjnTrETTRRjdjQ74VjoMJ365drmPkr4qN5CE6ztJ"
  "bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu"
  "bob_prod_bob-admin_6BiuAbjqXBKMoPe4kBFbSmDCV1p3zprcVrubyFtju118kiJ29n296EKiSxtcM8SBWk6X5RTpnYQdkKxdSHnAjWh_HXNWFe61d6cKaPstZ3qYTWXscuQ2TrfDXa1UWjhT1YKv"
  "bob_prod_bob-admin_5WciJzobAhwqBg9mGnufiVpP8YQgvkxKSa2qkm6RhvBX7zHGmKLvkMzCPxorABYn17ecfDWFAXNS8VH7R1kJvt88_4esgR8gBMyHk9tfni1DdvaZZRYAroncZNrj9SXack9GC"
)

# 43 never-launched epics (all have Phase 0 complete)
NEVER_LAUNCHED=(3 7 8 15 17 19 20 24 27 31 32 39 43 44 51 55 56 63 67 68 75 79 80 87 91 92 99 100 103 104 111 115 116 123 127 128 135 139 140 147 151 152 159)

# 13 failed epics (need re-run with fresh API keys)
FAILED=(26 47 66 73 86 94 101 108 114 129 134 148 155)

# Combine all epics for recovery
ALL_RECOVERY=("${NEVER_LAUNCHED[@]}" "${FAILED[@]}")

echo "=== Wave 7 Phase 1 Recovery Launch ==="
echo "Total epics to recover: ${#ALL_RECOVERY[@]}"
echo "Never launched: ${#NEVER_LAUNCHED[@]}"
echo "Failed (re-run): ${#FAILED[@]}"
echo "API keys available: ${#API_KEYS[@]}"
echo ""

# Launch all recovery epics
api_index=0
for epic_num in "${ALL_RECOVERY[@]}"; do
  EPIC_ID=$(printf "EPIC-W7-%03d" $epic_num)
  SCRIPT="_p1_$(printf "%03d" $epic_num).sh"
  
  # Check if script exists
  if [ ! -f "$SCRIPT" ]; then
    echo "WARNING: Script not found for $EPIC_ID, skipping..."
    continue
  fi
  
  # Rotate API key
  export BOBSHELL_API_KEY="${API_KEYS[$api_index]}"
  api_index=$(( (api_index + 1) % ${#API_KEYS[@]} ))
  
  echo "Launching $EPIC_ID (API key $api_index)..."
  bash "$SCRIPT" > "logs/phase1_epic_${epic_num}.log" 2>&1 &
  
  # 12-second delay to prevent VM overload
  sleep 12
done

echo ""
echo "=== All recovery epics launched ==="
echo "Monitor with: tail -f logs/phase1_epic_*.log"
echo "Check completion: ls docs/brain/EPIC-W7-*/00-scope.md | wc -l"
