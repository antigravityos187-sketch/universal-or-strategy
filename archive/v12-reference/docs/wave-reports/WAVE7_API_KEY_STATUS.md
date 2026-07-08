# Wave 7 API Key Status

**Last Updated**: 2026-06-24T01:57:00Z  
**Total Keys**: 15  
**Active Keys**: 12  
**Exhausted Keys**: 3  
**Revoked Keys**: 1 (jessica - DELETED)

---

## Active API Keys (12 Total)

These keys have bobcoins remaining and are available for Phase 2+:

1. **alprofit** - Active
2. **bob (5)** - Active
3. **danielmccullum** - Active (NEW - added 2026-06-24)
4. **davidflynn.t** - Active (NEW - added 2026-06-24)
5. **iyanajackson** - Active
6. **jimbianco** - Active
7. **rakaarababa** - Active
8. **ranirabah (1)** - Active
9. **sammy96** - Active
10. **snyder.johnson** - Active
11. **stephanielane22** - Active
12. **yasminegrabi** - Active (NEW - added 2026-06-24)

---

## Exhausted API Keys (3 Total)

These keys have spent 160/160 bobcoins and should be excluded from rotation:

1. **danfarah** - Exhausted (160/160 bobcoins spent)
2. **jimmydore** - Exhausted (160/160 bobcoins spent)
3. **pepeescobar** - Exhausted (160/160 bobcoins spent)

---

## Revoked API Keys (1 Total - DELETED)

1. **jessica** - ❌ REVOKED and DELETED from `docs/API/`
   - **Status**: Authentication failure ("budget exceeded" error)
   - **Action**: Removed from filesystem on 2026-06-24
   - **Impact**: Caused 9 Phase 1.5 failures (epics 015, 047, 063, 079, 095, 111, 127, 143, 159)
   - **Resolution**: Fixed using Building-Blocks Method (copied script 002)

---

## New Keys Added (3 Total)

Recent additions to the API key pool:

1. **danielmccullum** - Created 2026-06-24T01:18:35Z
2. **davidflynn.t** - Created 2026-06-24T01:13:17Z
3. **yasminegrabi** - Created 2026-06-24T01:22:28Z

**Note**: User mentioned bringing a replacement for jessica. When added, update this document.

---

## API Key Rotation Strategy

### Current Rotation (12-Key)

**Formula**: `epic_num % 12`

**Rotation Order**:
1. alprofit (index 0)
2. bob (5) (index 1)
3. danielmccullum (index 2)
4. davidflynn.t (index 3)
5. iyanajackson (index 4)
6. jimbianco (index 5)
7. rakaarababa (index 6)
8. ranirabah (1) (index 7)
9. sammy96 (index 8)
10. snyder.johnson (index 9)
11. stephanielane22 (index 10)
12. yasminegrabi (index 11)

**Excluded from Rotation**:
- danfarah (exhausted)
- jimmydore (exhausted)
- pepeescobar (exhausted)
- jessica (revoked and deleted)

### Epic-to-Key Mapping Examples

| Epic | Formula | Index | Key |
|------|---------|-------|-----|
| EPIC-W7-001 | 1 % 12 | 1 | bob (5) |
| EPIC-W7-002 | 2 % 12 | 2 | danielmccullum |
| EPIC-W7-003 | 3 % 12 | 3 | davidflynn.t |
| EPIC-W7-012 | 12 % 12 | 0 | alprofit |
| EPIC-W7-013 | 13 % 12 | 1 | bob (5) |
| EPIC-W7-024 | 24 % 12 | 0 | alprofit |

---

## Bobcoin Capacity Analysis

### Per-Key Capacity
- **Standard Allocation**: 160 bobcoins per key
- **Active Keys**: 12
- **Total Capacity**: 1,920 bobcoins

### Phase 1.5 Usage
- **Epics Completed**: 161
- **Estimated Cost**: ~8 bobcoins per epic
- **Total Spent**: ~1,288 bobcoins
- **Remaining**: ~632 bobcoins

### Phase 2 Projection
- **Epics**: 161
- **Estimated Cost**: ~10 bobcoins per epic (architecture planning)
- **Total Needed**: ~1,610 bobcoins
- **Status**: ⚠️ **INSUFFICIENT** - Need ~978 more bobcoins

**Recommendation**: Add 7-8 more API keys before Phase 2 to ensure sufficient capacity.

---

## Key Management Commands

### List All Keys
```bash
ls -1 docs/API/*.json | xargs -n1 basename | sed 's/.json$//'
```

### Count Active Keys
```bash
# Total keys
ls -1 docs/API/*.json | wc -l

# Active keys (excluding exhausted)
# Manual count: Total - 3 (exhausted)
```

### Verify Key Exists
```bash
# Check if key file exists
ls docs/API/keyname.json

# Read key details
cat docs/API/keyname.json | jq .
```

### Add New Key
```bash
# Create new key file
cat > docs/API/newkey.json << 'EOF'
{
  "name": "newkey",
  "createdAt": "2026-06-24T02:00:00Z",
  "apikey": "bob_prod_bob-admin_..."
}
EOF
```

### Remove Key
```bash
# Delete key file
rm docs/API/keyname.json

# Verify deletion
ls docs/API/keyname.json 2>&1 || echo "✅ Key deleted"
```

---

## Phase 1.5 Jessica Incident

### Timeline
1. **2026-06-23**: Phase 1.5 launched with 16-key rotation (included jessica)
2. **2026-06-24T00:30**: VM shutdown, 97/161 complete
3. **2026-06-24T01:00**: Recovery launched, 152/161 complete
4. **2026-06-24T01:30**: 9 epics failed with "budget exceeded" error
5. **2026-06-24T01:45**: Root cause identified - jessica key revoked
6. **2026-06-24T01:49**: Fixed using Building-Blocks Method (copied script 002)
7. **2026-06-24T01:52**: All 9 epics completed successfully (161/161)
8. **2026-06-24T01:57**: jessica.json deleted from docs/API/

### Affected Epics
All epics at rotation index 15 (old 16-key rotation):
- EPIC-W7-015
- EPIC-W7-047
- EPIC-W7-063
- EPIC-W7-079
- EPIC-W7-095
- EPIC-W7-111
- EPIC-W7-127
- EPIC-W7-143
- EPIC-W7-159

### Resolution
Used Building-Blocks Method to regenerate scripts:
```bash
for epic in 015 047 063 079 095 111 127 143 159; do
  cp _p1_5_002.sh _p1_5_$epic.sh
  sed -i "s/EPIC-W7-002/EPIC-W7-$epic/g" _p1_5_$epic.sh
  sed -i "s/phase1_5_msg_001/phase1_5_msg_$epic/g" _p1_5_$epic.sh
  chmod +x _p1_5_$epic.sh
done
```

### Lessons Learned
1. ✅ **Verify API keys before wave launch** - Check authentication status
2. ✅ **Exclude revoked keys immediately** - Don't wait for failures
3. ✅ **Building-Blocks Method works** - Shell commands more reliable than Python
4. ✅ **Monitor error messages carefully** - "Budget exceeded" was misleading (actually auth failure)

---

## Phase 2 Preparation

### Current Status
- ✅ 12 active API keys available
- ⚠️ ~632 bobcoins remaining (insufficient for Phase 2)
- ✅ jessica key removed from rotation
- ⏳ Awaiting replacement key from user

### Action Items
1. **Add replacement key** for jessica (user will provide)
2. **Add 6-7 more keys** to reach ~2,500 bobcoin capacity
3. **Update rotation formula** to use new key count
4. **Test new keys** with pilot epics before full wave
5. **Document new keys** in this status file

### Recommended Key Count
- **Minimum**: 15 keys (2,400 bobcoins) - Tight but workable
- **Optimal**: 18 keys (2,880 bobcoins) - Comfortable buffer
- **Maximum**: 20 keys (3,200 bobcoins) - Generous reserve

---

## Appendix: Key File Format

### Standard Format
```json
{
  "name": "keyname",
  "createdAt": "2026-06-24T00:00:00Z",
  "apikey": "bob_prod_bob-admin_..."
}
```

### Required Fields
- `name`: Key identifier (matches filename without .json)
- `createdAt`: ISO 8601 timestamp
- `apikey`: Full Bob API key starting with `bob_prod_bob-admin_`

### Naming Convention
- Lowercase only
- No spaces (use dots or underscores)
- Match filename: `keyname.json` → `"name": "keyname"`

---

**Document Status**: ✅ CURRENT  
**Next Update**: After replacement key added  
**Maintained By**: Autonomous Refactor Mode