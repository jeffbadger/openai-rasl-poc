# Planning — Durable Memory

Rules for using and writing durable memory. Load this file when the request contains a non-empty `# Durable Memory` section, or when the response contract includes `DurableMemoryWrites`.

---

## Usage rules

The `# Durable Memory` section is approved cross-task context supplied by the host.

- Use only when relevant to the current task and compatible with the current grounded surface.
- Do not infer facts, values, identifiers, or structure beyond what is explicitly present.
- Durable memory parameter names and method descriptions are **not** a source of control names, field identifiers, or UI structure. Every `ControlName` and `ElementReferenceId` must resolve to an entry actually present in the supplied hierarchy (grounding rule, `planning-core.md` §3).

## Callable automation invocation

When durable memory contains `automationSignatures` entries, treat every method object as a fully approved callable automation.

When a listed automation semantically matches the current goal or a required sub-step:
- `isApplicationMethod: true` → `ParentObject` = application name, add that application to `Includes`, call as `AppName.MethodName(...)`
- `isApplicationMethod: false` or absent → `ParentObject` = `"Project"`, do not add to `Includes`, call as `MethodName.Run(...)`
- Map parameters from current context to the declared parameter names in the signature.

A durable memory `MethodStep` takes precedence over a `TodoStep` when the automation's intent matches the required work.

**Cross-surface prohibition (MUST):** When the current turn is grounded in an application hierarchy, do not invoke a durable memory automation from a different application domain as an intermediate step. Values needed from another surface are runtime input parameters — plan steps to accept and enter them directly (`StaticValue: null`). Do not substitute a `TodoStep` for the blocked lookup.

## Durable memory write policy

When the response contract includes `DurableMemoryWrites`:
- Write only compact, reusable facts that later tasks are likely to need.
- Write only values grounded in the current hierarchy, explicit metadata, or generated method plan.
- Prefer structured values over prose.
- Do not write raw business record values, freeform extracted text, credentials, PII, or unapproved key types.
- If no approved durable-memory item is discovered this turn, omit `DurableMemoryWrites` or return an empty list.

For the callable first-turn `callableAutomation` write requirement, see `planning-callable.md` §2.
