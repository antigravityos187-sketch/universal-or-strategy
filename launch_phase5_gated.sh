#!/bin/bash
# Gated Sequential Workflow: TICKET -> VALIDATE -> TICKET -> VALIDATE

wait_for_completion() {
    while screen -list | grep -q "$1"; do sleep 10; done
}

check_validation_result() {
    local verification_file="docs/brain/EPIC-CCN-$1/ticket-$2-verification.md"
    if grep -q "Verdict.*FAIL" "$verification_file"; then
        echo "[FAIL] TICKET-$2 validation FAILED. Fix before proceeding."
        return 1
    fi
    return 0
}


# EPIC-CCN-107
screen -dmS p5_107_t1 bash -l _p5_107_t1.sh
wait_for_completion p5_107_t1
screen -dmS p5v_107_t1 bash -l _p5v_107_t1.sh
wait_for_completion p5v_107_t1
check_validation_result 107 1 || exit 1
screen -dmS p5_107_t2 bash -l _p5_107_t2.sh
wait_for_completion p5_107_t2
screen -dmS p5v_107_t2 bash -l _p5v_107_t2.sh
wait_for_completion p5v_107_t2
check_validation_result 107 2 || exit 1
screen -dmS p5_107_t3 bash -l _p5_107_t3.sh
wait_for_completion p5_107_t3
screen -dmS p5v_107_t3 bash -l _p5v_107_t3.sh
wait_for_completion p5v_107_t3
check_validation_result 107 3 || exit 1
screen -dmS p5_107_t4 bash -l _p5_107_t4.sh
wait_for_completion p5_107_t4
screen -dmS p5v_107_t4 bash -l _p5v_107_t4.sh
wait_for_completion p5v_107_t4
check_validation_result 107 4 || exit 1
screen -dmS p5_107_t5 bash -l _p5_107_t5.sh
wait_for_completion p5_107_t5
screen -dmS p5v_107_t5 bash -l _p5v_107_t5.sh
wait_for_completion p5v_107_t5
check_validation_result 107 5 || exit 1
screen -dmS p5_107_t6 bash -l _p5_107_t6.sh
wait_for_completion p5_107_t6
screen -dmS p5v_107_t6 bash -l _p5v_107_t6.sh
wait_for_completion p5v_107_t6
check_validation_result 107 6 || exit 1
screen -dmS p6_107 bash -l _p6_107.sh
wait_for_completion p6_107

# EPIC-CCN-108
screen -dmS p5_108_t1 bash -l _p5_108_t1.sh
wait_for_completion p5_108_t1
screen -dmS p5v_108_t1 bash -l _p5v_108_t1.sh
wait_for_completion p5v_108_t1
check_validation_result 108 1 || exit 1
screen -dmS p5_108_t2 bash -l _p5_108_t2.sh
wait_for_completion p5_108_t2
screen -dmS p5v_108_t2 bash -l _p5v_108_t2.sh
wait_for_completion p5v_108_t2
check_validation_result 108 2 || exit 1
screen -dmS p5_108_t3 bash -l _p5_108_t3.sh
wait_for_completion p5_108_t3
screen -dmS p5v_108_t3 bash -l _p5v_108_t3.sh
wait_for_completion p5v_108_t3
check_validation_result 108 3 || exit 1
screen -dmS p5_108_t4 bash -l _p5_108_t4.sh
wait_for_completion p5_108_t4
screen -dmS p5v_108_t4 bash -l _p5v_108_t4.sh
wait_for_completion p5v_108_t4
check_validation_result 108 4 || exit 1
screen -dmS p5_108_t5 bash -l _p5_108_t5.sh
wait_for_completion p5_108_t5
screen -dmS p5v_108_t5 bash -l _p5v_108_t5.sh
wait_for_completion p5v_108_t5
check_validation_result 108 5 || exit 1
screen -dmS p6_108 bash -l _p6_108.sh
wait_for_completion p6_108

# EPIC-CCN-109
screen -dmS p5_109_t1 bash -l _p5_109_t1.sh
wait_for_completion p5_109_t1
screen -dmS p5v_109_t1 bash -l _p5v_109_t1.sh
wait_for_completion p5v_109_t1
check_validation_result 109 1 || exit 1
screen -dmS p5_109_t2 bash -l _p5_109_t2.sh
wait_for_completion p5_109_t2
screen -dmS p5v_109_t2 bash -l _p5v_109_t2.sh
wait_for_completion p5v_109_t2
check_validation_result 109 2 || exit 1
screen -dmS p5_109_t3 bash -l _p5_109_t3.sh
wait_for_completion p5_109_t3
screen -dmS p5v_109_t3 bash -l _p5v_109_t3.sh
wait_for_completion p5v_109_t3
check_validation_result 109 3 || exit 1
screen -dmS p5_109_t4 bash -l _p5_109_t4.sh
wait_for_completion p5_109_t4
screen -dmS p5v_109_t4 bash -l _p5v_109_t4.sh
wait_for_completion p5v_109_t4
check_validation_result 109 4 || exit 1
screen -dmS p6_109 bash -l _p6_109.sh
wait_for_completion p6_109

# EPIC-CCN-111
screen -dmS p5_111_t1 bash -l _p5_111_t1.sh
wait_for_completion p5_111_t1
screen -dmS p5v_111_t1 bash -l _p5v_111_t1.sh
wait_for_completion p5v_111_t1
check_validation_result 111 1 || exit 1
screen -dmS p5_111_t2 bash -l _p5_111_t2.sh
wait_for_completion p5_111_t2
screen -dmS p5v_111_t2 bash -l _p5v_111_t2.sh
wait_for_completion p5v_111_t2
check_validation_result 111 2 || exit 1
screen -dmS p5_111_t3 bash -l _p5_111_t3.sh
wait_for_completion p5_111_t3
screen -dmS p5v_111_t3 bash -l _p5v_111_t3.sh
wait_for_completion p5v_111_t3
check_validation_result 111 3 || exit 1
screen -dmS p6_111 bash -l _p6_111.sh
wait_for_completion p6_111

# EPIC-CCN-112
screen -dmS p5_112_t1 bash -l _p5_112_t1.sh
wait_for_completion p5_112_t1
screen -dmS p5v_112_t1 bash -l _p5v_112_t1.sh
wait_for_completion p5v_112_t1
check_validation_result 112 1 || exit 1
screen -dmS p5_112_t2 bash -l _p5_112_t2.sh
wait_for_completion p5_112_t2
screen -dmS p5v_112_t2 bash -l _p5v_112_t2.sh
wait_for_completion p5v_112_t2
check_validation_result 112 2 || exit 1
screen -dmS p5_112_t3 bash -l _p5_112_t3.sh
wait_for_completion p5_112_t3
screen -dmS p5v_112_t3 bash -l _p5v_112_t3.sh
wait_for_completion p5v_112_t3
check_validation_result 112 3 || exit 1
screen -dmS p5_112_t4 bash -l _p5_112_t4.sh
wait_for_completion p5_112_t4
screen -dmS p5v_112_t4 bash -l _p5v_112_t4.sh
wait_for_completion p5v_112_t4
check_validation_result 112 4 || exit 1
screen -dmS p5_112_t5 bash -l _p5_112_t5.sh
wait_for_completion p5_112_t5
screen -dmS p5v_112_t5 bash -l _p5v_112_t5.sh
wait_for_completion p5v_112_t5
check_validation_result 112 5 || exit 1
screen -dmS p5_112_t6 bash -l _p5_112_t6.sh
wait_for_completion p5_112_t6
screen -dmS p5v_112_t6 bash -l _p5v_112_t6.sh
wait_for_completion p5v_112_t6
check_validation_result 112 6 || exit 1
screen -dmS p6_112 bash -l _p6_112.sh
wait_for_completion p6_112

# EPIC-CCN-113
screen -dmS p5_113_t1 bash -l _p5_113_t1.sh
wait_for_completion p5_113_t1
screen -dmS p5v_113_t1 bash -l _p5v_113_t1.sh
wait_for_completion p5v_113_t1
check_validation_result 113 1 || exit 1
screen -dmS p5_113_t2 bash -l _p5_113_t2.sh
wait_for_completion p5_113_t2
screen -dmS p5v_113_t2 bash -l _p5v_113_t2.sh
wait_for_completion p5v_113_t2
check_validation_result 113 2 || exit 1
screen -dmS p5_113_t3 bash -l _p5_113_t3.sh
wait_for_completion p5_113_t3
screen -dmS p5v_113_t3 bash -l _p5v_113_t3.sh
wait_for_completion p5v_113_t3
check_validation_result 113 3 || exit 1
screen -dmS p5_113_t4 bash -l _p5_113_t4.sh
wait_for_completion p5_113_t4
screen -dmS p5v_113_t4 bash -l _p5v_113_t4.sh
wait_for_completion p5v_113_t4
check_validation_result 113 4 || exit 1
screen -dmS p5_113_t5 bash -l _p5_113_t5.sh
wait_for_completion p5_113_t5
screen -dmS p5v_113_t5 bash -l _p5v_113_t5.sh
wait_for_completion p5v_113_t5
check_validation_result 113 5 || exit 1
screen -dmS p6_113 bash -l _p6_113.sh
wait_for_completion p6_113

# EPIC-CCN-114
screen -dmS p5_114_t1 bash -l _p5_114_t1.sh
wait_for_completion p5_114_t1
screen -dmS p5v_114_t1 bash -l _p5v_114_t1.sh
wait_for_completion p5v_114_t1
check_validation_result 114 1 || exit 1
screen -dmS p6_114 bash -l _p6_114.sh
wait_for_completion p6_114
