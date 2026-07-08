#!/bin/bash
# Wave 7 Phase 1 Final Launch Script
# Uses successful pilot mechanics with API key rotation
# 12-second delays between launches (no batching)

set -e

echo "=== Wave 7 Phase 1 Final Launch ==="
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

# API keys from docs/API (15 valid keys)
API_KEYS=(
    "bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu"  # alprofit
    "bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ"  # bob
    "bob_prod_bob-admin_5PbEKjG9uqQAMCizgY3pPmDxRFiTW6XJvBvp3RDcYGbT6vk5tE8FJmLUCWsT1WfH6nn7WYoxMHrXe3GfhFo8LPgi_5dYLFXNSc2SBcHL9ZrQXwxj1XrvbotdUsWJXVTv13u2a"  # danfarah
    "bob_prod_bob-admin_6BiuAbjqXBKMoPe4kBFbSmDCV1p3zprcVrubyFtju118kiJ29n296EKiSxtcM8SBWk6X5RTpnYQdkKxdSHnAjWh_HXNWFe61d6cKaPstZ3qYTWXscuQ2TrfDXa1UWjhT1YKv"  # danielmccullum
    "bob_prod_bob-admin_3h6asj27KMtNwpBXWi1m9aL7rJa7ZQqPwCTBE4kSTa7V9ZZQZXw9Fot4umQDChd476i5wu7z4njG2ggdJrUQQ6uF_9axjeHVDZUBdsKxMWJm5ZwiNuGwsHz6TpjbaF8VeX8tk"  # davidflynn.t
    "bob_prod_bob-admin_5WciJzobAhwqBg9mGnufiVpP8YQgvkxKSa2qkm6RhvBX7zHGmKLvkMzCPxorABYn17ecfDWFAXNS8VH7R1kJvt88_4esgR8gBMyHk9tfni1DdvaZZRYAroncZNrj9SXack9GC"  # jimbianco
    "bob_prod_bob-admin_58sbZY3cBGWbej6dmAvwYeRTKyuceZJppgm4vYoS7bb2yzKqFxwAmzsR46D6G86LVJWNBmsUaZLBpMgRpiZyPQDf_GpwXFSKRi7nWHCJP2m1S6guZ1Y4kzUKBR9C1mrkKQm3s"  # jimmydore
    "bob_prod_bob-admin_4tdFq99zrsvGGgqpLmsaDid9QqycnQT74EtvTFttZpWcJdWW5L3VEQuCTsQxM1GTWDCd8HWkPW9jcWPFqYp5hW9v_8TSHVEQRkt3DbE6zuqMQHoajMzLtuUUYdUxTxSrofQMg"  # pepeescobar
    "bob_prod_bob-admin_aRSjzM4xwaEhbcjDdViPqh3giwmvtQksbGerdHvRxq8MPyN2X7KHUU9q6H9DYDBj2YaJwhkgDci2HcT1gRbS9d6_9MHxQ1wMuJVJYeJG2gbRe4NCDCAdf2GBd4wKLhQMg1hS"  # rakaarababa
    "bob_prod_bob-admin_hXrAgTSbvVHGpmUxqohSv8SirgVeXpEBSoF1wyb8xBQz2PBMmgyKfozT2kJP5RuCbqBVsW91Z6pp8auuGBjb53P_2qJE8WD7whnc8nYcahv4AevV8ekHvP1K4zbYggeoSvZ2"  # randyyoung
    "bob_prod_bob-admin_2aNT7CZ3HecXSysJasUajLjgd5mGiXmmmLMxAk3ARKayj6hzL53KRLBTSZYvVZyptBa8ydotXWvezrgdru7v8TfW_DjcCaV1W6wMZkjQ2sjZrN8m8Hnw9GS5iVFkGqb34eKEJ"  # ranirabah
    "bob_prod_bob-admin_22HihoispYfg9TBX2smEUD6b18c9zwRRFnssCBDwTquLYiH4bvHLkntgiNVgt5DZtcfSUqE7LDbMBJxrb9W6cCQc_AKDtJW7uTVi1ZpoDxktCmz2WNcvi2REwiTe88STW7h4J"  # sammy96
    "bob_prod_bob-admin_2FTTdxZo3mEs7ek4rbpBLVdpkTinfTdgG6Zj2CK9D2A7ct7TwUi1CyQSaHwqEozi9npR6Go4BLkBzAyxQzaWpaii_B1y7Ji37WbeKFZgREwNCqjQEJCdzqfhwpCN9Rfa1BiMN"  # snyder.johnson
    "bob_prod_bob-admin_2SC4VtHL6svY4W1mLFsfLCztVEc6wzuuoXFevyjQCEEojgcH7LLtWUBQiwZ9Q2q6cy8Hmrurpm1KkpXqaoSiKEez_5Vu6PjnTrETTRRjdjQ74VjoMJ365drmPkr4qN5CE6ztJ"  # stephanielane22
    "bob_prod_bob-admin_3SKCJRKM5kTYHCCbLL5KcebgMYk1ZP2CuW4pmQkVPfhzcTqDMiRQAiauiZ2RDMAZ3RY2MihHAJkzzSPJsYvK7vjW_3cM1RH5zLE5owNwVMg2Gnc9ScdPKyNnWmnJy6nXXJH6i"  # yasminegrabi
)

# Exclude completed pilots
EXCLUDE_EPICS="100 024 017"

# Counters
LAUNCHED=0
SKIPPED=0
API_INDEX=0

# Launch all Phase 1 scripts with API rotation
for script in _p1_[0-9][0-9][0-9].sh _p1_[0-9][0-9].sh _p1_[0-9].sh; do
    [ -f "$script" ] || continue
    
    EPIC_NUM=$(echo "$script" | sed 's/_p1_\([0-9]*\)\.sh/\1/' | sed 's/^0*//')
    
    # Skip pilots
    SKIP=0
    for EXCLUDE in $EXCLUDE_EPICS; do
        [ "$EPIC_NUM" = "$EXCLUDE" ] && SKIP=1 && break
    done
    if [ $SKIP -eq 1 ]; then
        echo "⏭️  Skipping $script (pilot complete)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Check Phase 0 complete
    EPIC_DIR="docs/brain/EPIC-W7-$(printf '%03d' $EPIC_NUM)"
    if [ ! -f "$EPIC_DIR/00-hotspots.md" ]; then
        echo "⚠️  Skipping $script (no Phase 0)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Check Phase 1 not already complete
    if [ -f "$EPIC_DIR/00-scope.md" ]; then
        echo "✅ Skipping $script (Phase 1 complete)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Rotate API key
    CURRENT_KEY="${API_KEYS[$API_INDEX]}"
    API_INDEX=$(( (API_INDEX + 1) % ${#API_KEYS[@]} ))
    
    # Update script with current API key (same mechanics as successful pilot)
    sed -i "s/export BOBSHELL_API_KEY=.*/export BOBSHELL_API_KEY='$CURRENT_KEY'/" "$script"
    
    # Launch epic in background
    echo "🚀 Launching $script (EPIC-W7-$EPIC_NUM) [API key $API_INDEX/15]"
    bash "$script" > "logs/phase1_epic_${EPIC_NUM}.log" 2>&1 &
    LAUNCHED=$((LAUNCHED + 1))
    
    # 12-second delay (same as successful pilot)
    sleep 12
done

echo ""
echo "=== Launch Summary ==="
echo "Launched: $LAUNCHED epics"
echo "Skipped: $SKIPPED epics"
echo "End time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""
echo "Monitor progress:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-* -name \"00-scope.md\" | wc -l'"

# Made with Bob
