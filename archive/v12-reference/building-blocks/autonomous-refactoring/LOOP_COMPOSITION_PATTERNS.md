# Loop Composition Patterns - Dynamic Orchestration

## Overview

The autonomous refactoring system uses **four composable loops** that can be arranged in different configurations based on task requirements. Each loop has a measurable goal and can function as either an outer loop (orchestrator) or inner loop (worker).

## The Four Building Block Loops

| Loop | Goal | Duration | Scope |
|------|------|----------|-------|
| **local-loop** | 5/5 checks | 2-5 min | Single file |
| **pr-loop** | 100/100 PHS | 15-45 min | Pull request |
| **epic-run** | CYC ≤8 | 15-25 min | Single method |
| **epic-loop** | All methods ≤8 | 6-12 weeks | Multiple epics |

## Composition Principle

**Key Insight**: Loops can be rearranged based on what you're optimizing for:

- **Optimize for Quality** → pr-loop as outer loop
- **Optimize for Complexity** → epic-loop as outer loop
- **Optimize for Speed** → local-loop as outer loop (fast feedback)
- **Optimize for Scale** → epic-loop with parallel execution

## Configuration Patterns

### Pattern 1: Quality-First (Current Default)

**Use Case**: Ensure every change meets 100/100 PHS before proceeding

```
epic-loop (Outer)
└─ epic-run (Middle)
    └─ pr-loop (Inner)
        └─ local-loop (Innermost)
```

**Flow**:
1. epic-loop iterates through all epics
2. epic-run handles single method refactoring
3. pr-loop ensures quality perfection (100/100)
4. local-loop validates each fix (5/5)

**Pros**:
- ✅ Highest quality (every commit is perfect)
- ✅ No technical debt accumulation
- ✅ Jane Street alignment enforced

**Cons**:
- ⚠️ Slower (waits for 100/100 per ticket)
- ⚠️ Bot latency impacts throughput

**Best For**: Production systems, critical infrastructure, HFT code

---

### Pattern 2: Speed-First (Fast Feedback)

**Use Case**: Rapid iteration with batch quality checks

```
epic-loop (Outer)
└─ epic-run (Middle)
    └─ local-loop (Inner)
        └─ pr-loop (Deferred to end)
```

**Flow**:
1. epic-loop iterates through all epics
2. epic-run handles single method refactoring
3. local-loop validates each change (5/5)
4. pr-loop runs ONCE at the end (batch quality)

**Pros**:
- ✅ Faster (no bot waiting per ticket)
- ✅ Immediate feedback (local checks only)
- ✅ Batch quality verification

**Cons**:
- ⚠️ Quality debt accumulates
- ⚠️ Harder to isolate failures
- ⚠️ Large PR at end (>10k diff risk)

**Best For**: Prototyping, exploratory refactoring, non-critical code

---

### Pattern 3: Complexity-First (Surgical Precision)

**Use Case**: Focus on complexity reduction, defer quality

```
epic-loop (Outer)
└─ local-loop (Inner - complexity check only)
    └─ pr-loop (Deferred to end)
```

**Flow**:
1. epic-loop iterates through all epics
2. local-loop validates complexity target (CYC ≤8)
3. Skip epic-run phases (direct extraction)
4. pr-loop runs ONCE at the end

**Pros**:
- ✅ Fastest (minimal validation)
- ✅ Focus on single metric (CYC)
- ✅ Good for bulk refactoring

**Cons**:
- ⚠️ No architectural review
- ⚠️ Risk of poor extraction strategy
- ⚠️ Quality debt accumulates

**Best For**: Technical debt sprints, complexity audits, batch refactoring

---

### Pattern 4: Quality-Only (No Refactoring)

**Use Case**: Drive existing PR to 100/100 without new changes

```
pr-loop (Outer - standalone)
└─ local-loop (Inner)
```

**Flow**:
1. pr-loop extracts bot forensics
2. local-loop validates each fix (5/5)
3. Repeat until PHS = 100/100

**Pros**:
- ✅ Simplest (no epic complexity)
- ✅ Fast (no refactoring overhead)
- ✅ Focused (quality only)

**Cons**:
- ⚠️ No complexity reduction
- ⚠️ No architectural improvement

**Best For**: PR rescue, quality gates, pre-merge cleanup

---

### Pattern 5: Parallel Execution (Scale-First)

**Use Case**: Process multiple epics simultaneously

```
epic-loop (Outer - 3 clusters)
├─ epic-run (Cluster 1: SIMA)
│   └─ pr-loop → local-loop
├─ epic-run (Cluster 2: Orders)
│   └─ pr-loop → local-loop
└─ epic-run (Cluster 3: Lifecycle)
    └─ pr-loop → local-loop
```

**Flow**:
1. epic-loop spawns 3 parallel workers
2. Each epic-run operates independently
3. Batch F5 verification (test all 3 together)
4. Sequential merge (one at a time)

**Pros**:
- ✅ 64% faster (267 hours saved)
- ✅ Scales linearly (N clusters)
- ✅ File-level isolation (no conflicts)

**Cons**:
- ⚠️ Coordination overhead
- ⚠️ Merge complexity
- ⚠️ Requires careful cluster assignment

**Best For**: Large-scale refactoring (100+ epics), time-constrained projects

---

### Pattern 6: Hybrid (Quality + Speed)

**Use Case**: Balance quality and speed with strategic checkpoints

```
epic-loop (Outer)
└─ epic-run (Middle)
    ├─ local-loop (Per ticket - fast feedback)
    └─ pr-loop (Per epic - quality gate)
```

**Flow**:
1. epic-loop iterates through all epics
2. epic-run handles single method refactoring
3. local-loop validates each ticket (5/5)
4. pr-loop runs ONCE per epic (not per ticket)

**Pros**:
- ✅ Balanced (fast + quality)
- ✅ Strategic checkpoints (per epic)
- ✅ Smaller PRs (per epic, not per ticket)

**Cons**:
- ⚠️ More complex orchestration
- ⚠️ Requires careful checkpoint planning

**Best For**: Most production scenarios, balanced workflows

---

## Dynamic Configuration

### Task-Based Selection

```python
def select_loop_configuration(task):
    if task.priority == "quality":
        return QualityFirstPattern()  # Pattern 1
    elif task.priority == "speed":
        return SpeedFirstPattern()    # Pattern 2
    elif task.priority == "complexity":
        return ComplexityFirstPattern()  # Pattern 3
    elif task.priority == "scale":
        return ParallelPattern()      # Pattern 5
    else:
        return HybridPattern()        # Pattern 6 (default)
```

### Context-Based Selection

```python
def select_loop_configuration(context):
    if context.is_production:
        return QualityFirstPattern()  # Never compromise quality
    elif context.is_prototype:
        return SpeedFirstPattern()    # Fast iteration
    elif context.is_technical_debt:
        return ComplexityFirstPattern()  # Focus on CYC
    elif context.epic_count > 50:
        return ParallelPattern()      # Scale required
    else:
        return HybridPattern()        # Balanced approach
```

## Optimization Strategies

### When to Use Each Pattern

| Scenario | Pattern | Rationale |
|----------|---------|-----------|
| Production HFT code | Quality-First | Zero tolerance for defects |
| Prototype/POC | Speed-First | Fast feedback loop |
| Technical debt sprint | Complexity-First | Focus on CYC reduction |
| Large-scale refactor (100+ epics) | Parallel | Time savings critical |
| PR rescue | Quality-Only | No new changes, just fix |
| Balanced workflow | Hybrid | Best of both worlds |

### Performance Comparison

| Pattern | Time (165 epics) | Quality | Complexity | Risk |
|---------|------------------|---------|------------|------|
| Quality-First | 415h | 100/100 | CYC ≤8 | Low |
| Speed-First | 250h | Deferred | CYC ≤8 | Medium |
| Complexity-First | 200h | Deferred | CYC ≤8 | High |
| Quality-Only | N/A | 100/100 | No change | Low |
| Parallel | 148h | 100/100 | CYC ≤8 | Medium |
| Hybrid | 300h | 100/100 | CYC ≤8 | Low |

## Implementation Examples

### Example 1: Quality-First (Current)

```bash
# Outer loop: epic-loop
/epic-loop 19 168

# For each epic, epic-run calls:
#   - pr-loop (per ticket)
#     - local-loop (per fix)
```

### Example 2: Speed-First

```bash
# Outer loop: epic-loop
/epic-loop 19 168 --fast-mode

# For each epic, epic-run calls:
#   - local-loop (per ticket)
# After all epics:
#   - pr-loop (batch quality)
```

### Example 3: Parallel Execution

```bash
# Outer loop: epic-loop (3 clusters)
# Window 1:
/epic-loop 19 73 --cluster SIMA

# Window 2:
/epic-loop 74 128 --cluster Orders

# Window 3:
/epic-loop 129 168 --cluster Lifecycle

# Batch F5 verification every 3 tickets
```

### Example 4: Hybrid

```bash
# Outer loop: epic-loop
/epic-loop 19 168 --hybrid-mode

# For each epic, epic-run calls:
#   - local-loop (per ticket - fast)
# After each epic:
#   - pr-loop (quality gate)
```

## Loop Composition Rules

### Rule 1: Measurable Goals

Every loop MUST have a quantifiable goal:
- local-loop: 5/5 checks
- pr-loop: 100/100 PHS
- epic-run: CYC ≤8
- epic-loop: All methods ≤8

### Rule 2: Single Responsibility

Each loop focuses on ONE concern:
- local-loop: File-level validation
- pr-loop: Quality perfection
- epic-run: Complexity reduction
- epic-loop: Multi-epic orchestration

### Rule 3: Composability

Loops can nest in any order as long as:
- Inner loop goal ⊆ Outer loop goal
- No circular dependencies
- Clear handoff protocol

### Rule 4: Idempotency

Running a loop multiple times with same input produces same output:
- local-loop: Same file → same 5/5 result
- pr-loop: Same PR → same 100/100 result
- epic-run: Same method → same CYC ≤8 result

### Rule 5: Failure Isolation

Loop failures don't cascade:
- local-loop fails → retry local-loop
- pr-loop fails → retry pr-loop
- epic-run fails → retry epic-run
- epic-loop fails → retry from failed epic

## Advanced Patterns

### Pattern 7: Conditional Nesting

**Use Case**: Adapt loop structure based on runtime conditions

```python
def adaptive_loop(epic):
    if epic.complexity > 30:
        # High complexity: use full pipeline
        return epic_run(epic, use_pr_loop=True)
    else:
        # Low complexity: skip pr-loop
        return epic_run(epic, use_pr_loop=False)
```

### Pattern 8: Progressive Enhancement

**Use Case**: Start fast, add quality checks incrementally

```python
def progressive_loop(epics):
    # Phase 1: Fast (local-loop only)
    for epic in epics[:50]:
        epic_run(epic, validation="local")
    
    # Phase 2: Balanced (local + pr per epic)
    for epic in epics[50:100]:
        epic_run(epic, validation="hybrid")
    
    # Phase 3: Quality (full pipeline)
    for epic in epics[100:]:
        epic_run(epic, validation="full")
```

### Pattern 9: Feedback-Driven

**Use Case**: Adjust loop structure based on failure rate

```python
def feedback_loop(epics):
    failure_rate = 0.0
    
    for epic in epics:
        if failure_rate < 0.1:
            # Low failures: use fast mode
            result = epic_run(epic, mode="fast")
        else:
            # High failures: use quality mode
            result = epic_run(epic, mode="quality")
        
        failure_rate = update_failure_rate(result)
```

## Configuration DSL

### Declarative Loop Configuration

```yaml
# config/loop-patterns.yaml

quality-first:
  outer: epic-loop
  middle: epic-run
  inner: pr-loop
  innermost: local-loop
  goal: "100/100 PHS + CYC ≤8"

speed-first:
  outer: epic-loop
  middle: epic-run
  inner: local-loop
  deferred: pr-loop
  goal: "CYC ≤8, batch quality"

parallel:
  outer: epic-loop
  clusters: 3
  per-cluster:
    middle: epic-run
    inner: pr-loop
    innermost: local-loop
  goal: "64% time savings"
```

### Runtime Selection

```bash
# Use predefined pattern
/epic-loop 19 168 --pattern quality-first

# Use custom pattern
/epic-loop 19 168 --pattern custom.yaml

# Override specific loops
/epic-loop 19 168 --pattern quality-first --skip-pr-loop
```

## Monitoring & Metrics

### Per-Pattern Metrics

Track effectiveness of each pattern:

```json
{
  "pattern": "quality-first",
  "epics_completed": 165,
  "total_time": "415h",
  "avg_phs": 100,
  "avg_cyc_reduction": "18→8",
  "failure_rate": 0.02
}
```

### Pattern Comparison Dashboard

```
Pattern          | Time  | Quality | Failures | Recommendation
-----------------|-------|---------|----------|----------------
Quality-First    | 415h  | 100/100 | 2%       | Production
Speed-First      | 250h  | Deferred| 8%       | Prototype
Complexity-First | 200h  | Deferred| 12%      | Tech Debt
Parallel         | 148h  | 100/100 | 5%       | Scale
Hybrid           | 300h  | 100/100 | 3%       | Balanced
```

## Conclusion

The autonomous refactoring system's **true power** lies in its **composable loop architecture**. By treating each loop as an independent building block with a measurable goal, we can:

1. **Optimize for different priorities** (quality, speed, complexity, scale)
2. **Adapt to task requirements** (production vs prototype)
3. **Balance trade-offs** (time vs quality)
4. **Scale efficiently** (parallel execution)
5. **Fail gracefully** (isolated retries)

**Key Insight**: The loops aren't just nested - they're **dynamically composable building blocks** that can be rearranged based on what you're optimizing for. This is the essence of the IBM Building Blocks philosophy applied to autonomous software engineering.

## References

- **README.md**: Overview and benefits
- **ARCHITECTURE.md**: Technical design
- **GETTING_STARTED.md**: Installation guide
- **Loop Commands**: `/epic-loop`, `/epic-run`, `/pr-loop`, `/local-loop`