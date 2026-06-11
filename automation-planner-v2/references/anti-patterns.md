# Anti-Patterns

Known planning failures observed in production, as wrong → right pairs. Always load this file. Each entry names the canonical rule that prevents it.

---

## 1. Business steps planned on an auth surface

**Situation:** Current screen is a login screen (credential fields grounded). Goal is a business task (search, read, process) whose controls are not grounded.

**Wrong:** Emit login steps + TodoSteps or speculative steps for the business task.
```
SetValue txtUsername, SetValue txtPassword, Click btnLogin,
TodoStep "Search for record"          ← violation
```

**Right:** Emit only credential entry + submit. Nothing else. `GoalCompleted = false`.
```
SetValue txtUsername, SetValue txtPassword, Click btnLogin   (stop)
```

**Rule:** Auth gate override, `planning-auth.md` §3 — loads based on what the *surface* contains, not what the goal says.

---

## 2. TodoStep or question instead of a selector step

**Situation:** Goal requests a value scoped by a category/type qualifier. The matching field sits in a selector-controlled region (radio group, tab, dropdown). Selectors are unselected — or disabled because a planned trigger (search/populate) hasn't fired yet.

**Wrong (a):** Match the field by name, ignore the selector context, emit a TodoStep when the value "can't be located."
**Wrong (b):** See the selector disabled at snapshot time and ask the user or emit a TodoStep — when a planned search trigger in this same turn explains the disable.

**Right:** Run the pre-match selector context check for every read target. Disabled selectors with a planned preceding trigger are "will be enabled" — plan the selector step after the trigger, then the read.
```
Click btnSearch → Click rbtnCreditCard → GetValue tbAccount
```

**Rule:** Pre-match selector context check, `application-matching.md` §2. A strong field-name match never skips this check.

---

## 3. Read-then-copy instead of direct output mapping

**Situation:** A UI value must be returned as a `Run.` output parameter and is used nowhere else.

**Wrong:** Two steps — read into the control name, then a ValueStep to transfer.
```
ApplicationValueStep GetValue txtLastName            ← intermediate
ValueStep txtLastName → Run.lastName                 ← redundant
```

**Right:** One step — the ApplicationValueStep maps directly.
```
ApplicationValueStep GetValue, SetValueControl: "Run.lastName"
```

A follow-on ValueStep is valid only when the same value is consumed in 2+ places.

**Rule:** Direct output mapping, `output-contract.md` Variable Minimization.

---

## 4. Claiming a rule was applied without evidence

**Situation:** Producing `PlanningTrace` in debug mode.

**Wrong:** `"RulesApplied": ["Variable minimization applied"]` while the steps contain the prohibited pattern.

**Right:** Every validation claim cites step numbers, control names, and counts from the actual output — and the plan is corrected if the evidence contradicts the claim.

**Rule:** ValidationResults evidence requirement, SKILL.md PlanningTrace section; `planning-core.md` §9.
