# Pattern: Data Loop

A data loop automation iterates over a collection of records or items and performs a repeated action on each one.

---

## When This Pattern Applies

- Goal involves processing "each" or "all" items in a list, table, grid, or result set
- Goal requires repeating the same action across multiple records (update each row, read each result, process each item)
- The collection is bounded (known count) or unbounded (iterate until no more items)

---

## Loop Type Selection

| Scenario | Loop type |
|---|---|
| Known count (fixed iterations) | `ForLoopStartStep` / `ForLoopEndStep` |
| PegaTable cursor iteration | `DoWhileLoopStartStep` / `DoWhileLoopEndStep` with MoveNext decision |
| Iterate while a condition holds, check at start | `WhileLoopStartStep` / `WhileLoopEndStep` |
| Iterate while a condition holds, check at end | `DoWhileLoopStartStep` / `DoWhileLoopEndStep` |
| Application grid rows (known last row) | `ForLoopStartStep` with row count as iteration bound |

When uncertain between While and DoWhile, prefer DoWhile for cursor-based iteration (the cursor must be moved before checking whether to continue).

---

## Step Shape: Bounded Collection (ForLoop)

```
1. Get collection size / last row / count                   — supporting
2. ForLoopStartStep (iterate from 1 to count)               — primary
3.   [Position to current item — click row, set index]      — supporting
4.   Read or write current item values                      — primary
5.   [Call service or callable per item]                    — primary (if needed)
6. ForLoopEndStep                                           — structural
```

---

## Step Shape: Cursor-Based Collection (DoWhileLoop + MoveNext)

```
1. MoveFirst                                                — supporting
2. [Decision on MoveFirst result — skip if empty]           — supporting
3. DoWhileLoopStartStep
4.   Read current item values                               — supporting or primary
5.   [Process / call service]                               — primary
6.   [Write results]                                        — primary
7.   MoveNext                                               — supporting
8.   Decision on MoveNext().Result                          — supporting
       true: [] (empty — continues automatically)
       false: exit or break
9. DoWhileLoopEndStep                                       — structural
```

---

## Planning Guidance

### Collection size
For ForLoop: emit a supporting step to get the count/last-row before the loop.
Use the result as the `IterationCount` — do not hardcode.

### Empty collection handling
When the collection may be empty, emit a decision on `MoveFirst().Result` or a count check before the loop.
If empty, skip the loop body entirely — do not emit an error or placeholder.

### Per-item positioning
In application grids, positioning to the current row may require:
- Clicking the row in the grid (if the grid supports row click selection)
- Using an index-based method on the grid control
- Using cursor methods if the grid is a PegaTable component

Emit the positioning step as `Tier: "supporting"` immediately before the per-item read/write steps.

### Exit conditions
For cursor iteration: the `MoveNext().Result = false` decision exits the loop.
For bounded iteration: the ForLoop exits automatically at the iteration count.
For conditional loops: emit the exit condition as part of the loop structure — do not emit a `JumpToLabelStep` to exit a loop unless the control flow genuinely requires it.

### GoalCompleted
A data loop typically completes its goal in a single turn when the full loop can be planned from the current surface.
`GoalCompleted = true` after the loop end step when all per-item work is included in the plan.
`GoalCompleted = false` only when per-item work requires a screen change and the next screen's controls are not yet grounded.

---

## AutomationCategory
- Setup steps before the loop (get count, navigate to list) → contributes to `"navigation"` classification only if no business value is captured in this turn
- Any turn containing the loop and per-item processing → `"core"`

---

## Common Variations

### Loop with conditional processing
Not every item requires the same action. Emit a decision step per item before the processing steps.
Base the decision on a value read from the current item.

### Loop with accumulation
When the goal requires summing, counting, or collecting values across all items:
Initialize an accumulator variable before the loop (ValueStep with StaticValue).
Update the accumulator inside the loop.
Use the accumulator result after the loop ends.

### Nested loops
When processing requires iterating a sub-collection per item (e.g., each order has multiple line items):
The outer loop iterates the primary collection.
The inner loop iterates the sub-collection.
Keep each loop's cursor/index variable distinct.
Apply the same pattern structure at each nesting level.

### Early exit
When the goal is to find the first matching item and stop:
Emit a decision inside the loop on the matching condition.
On match: capture the value and use `JumpToLabelStep` to exit the loop.
Place a `LabelStep` immediately after the loop end step as the exit target.
