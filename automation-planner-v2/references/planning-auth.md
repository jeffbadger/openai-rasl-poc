# Planning — Authentication

Authentication-specific planning rules. Load this file when:
- The goal is semantically an authentication goal (login, log in, sign in, authenticate)
- The current surface shows auth signals (credential fields, login buttons, SSO options)
- Auth signals are ambiguous or mixed

For core planning rules that apply to all turns, see `references/planning-core.md`.

---

## Table of Contents
1. [Goal Disambiguation](#1-goal-disambiguation)
2. [Mixed-Signal Auth Surface Confirmation](#2-mixed-signal-auth-surface-confirmation)
3. [Authentication Gate Override](#3-authentication-gate-override)
4. [Login-as-Goal GoalCompleted Gate](#4-login-as-goal-goalcompleted-gate)
5. [Authentication Step Minimalism](#5-authentication-step-minimalism)

---

## 1. Goal Disambiguation (MUST — evaluate before all other authentication rules)

If the primary/current goal is semantically an authentication goal (login, log in, sign in, authenticate, log into), route through credential-entry flow — regardless of any control on screen sharing that label.

- A "Login" button or "Sign In" link is the **final submit step** within credential entry — never a standalone fulfillment of an authentication goal.
- The goal is fulfilled only when credentials have been entered and the submit step has been performed.
- If the surface has a Login button but no credential input fields are grounded, treat the button as an advancement step (click to reveal the login form) — not goal completion.

**Credential field label tolerance:** A text input whose label, name, automationId, or placeholder contains "login", "log in", "sign in", or "username" is a **username credential field** — not evidence of goal fulfillment.

**Control type is determinative:** `Edit`, `TextBox`, or equivalent input → credential entry field. `Button`, `Hyperlink`, `MenuItem` → submit/navigation control.

---

## 2. Mixed-Signal Auth Surface Confirmation

Ask before classifying a surface as an authentication surface when the signals are ambiguous. Ambiguous signals include:

- A Login or Sign In button is present but no credential input fields are grounded in the hierarchy
- A federated, SSO, or "Sign in with [provider]" button is present — these navigate to an external auth provider rather than accepting local credentials
- The screen has a heading suggesting auth but the available controls don't match a standard credential entry form
- Multiple auth pathways are visible (e.g., local login fields alongside an SSO button)

When signals are ambiguous, call `ask_user` before generating any steps:
> "The current screen has [describe what is visible — e.g., a Sign In button but no credential fields / an SSO button alongside a username field]. What type of authentication should I plan for? Local credentials / SSO — click the [provider] button / Other: [user describes]"

When signals are clear — credential fields present and labelled, standard login form — classify as an authentication surface and proceed without asking.

---

## 3. Authentication Gate Override (MUST)

If the current surface is an authentication surface and the primary goal requires post-authentication work whose controls are not grounded:
1. Emit only the minimal grounded authentication steps (credential entry + submit).
2. Do not emit any other steps — no toolbox steps, no TodoSteps, no value capture unrelated to authentication.
3. Set `GoalCompleted = false`.
4. Stop after the authentication submit step.

**Exception:** `CandidateAutomation` may still be populated when a described public method matches the generated auth steps or the full primary goal sequence.

---

## 4. Login-as-Goal GoalCompleted Gate

When the primary goal is to log in, apply this gate sequence — stop at the first failure:

**Gate 1 — Password field grounded this turn:**
Is a password field grounded in the current surface, AND were both a password entry step and a submit step emitted this turn?
- No → `GoalCompleted = false`. Stop evaluating.
- Yes → Gate 2.

**Gate 2 — Username evidence exists:**
Was a username entry step emitted this turn, OR does `# Completed Steps` contain a username/login entry?
- No → `GoalCompleted = false`.
- Yes → `GoalCompleted = true`.

Gate 1 must be evaluated before Gate 2. A surface showing only a username field always fails Gate 1.

---

## 5. Authentication Step Minimalism

When generating auth-only output, use only controls grounded on the authentication surface.
Do not add credential-source steps unless the required method is grounded and explicitly requested.
Do not emit TodoSteps for obtaining credentials when grounded username/password controls exist.
