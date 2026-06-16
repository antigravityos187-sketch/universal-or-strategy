# Record Wave 2 v4 actual usage (3.23 bobcoins per epic for Phases 0-3)

$epics = @(
    @{api="bob.json"; epic="EPIC-CCN-107"},
    @{api="bob (1).json"; epic="EPIC-CCN-108"},
    @{api="bob (2).json"; epic="EPIC-CCN-109"},
    @{api="bob (3).json"; epic="EPIC-CCN-110"},
    @{api="bob (4).json"; epic="EPIC-CCN-111"},
    @{api="bob (5).json"; epic="EPIC-CCN-112"},
    @{api="bob (6).json"; epic="EPIC-CCN-113"},
    @{api="b.json"; epic="EPIC-CCN-114"},
    @{api="b (2).json"; epic="EPIC-CCN-115"}
)

foreach ($item in $epics) {
    Write-Host "Recording usage for $($item.epic)..."
    python scripts/wave2/api_balance_tracker.py record $item.api $item.epic 3.23 "0-3"
}

Write-Host "`nAll usage recorded. Showing summary..."
python scripts/wave2/api_balance_tracker.py summary

# Made with Bob
