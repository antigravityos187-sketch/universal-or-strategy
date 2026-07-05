> ## Documentation Index
> Fetch the complete documentation index at: https://docs.factory.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# Factory Missions

> Use Factory Missions to plan and execute large, multi-feature projects with structured orchestration. Describe your goal, collaborate on the plan, and let Droid manage the work.

<Frame>
  <img src="https://mintcdn.com/factory/XOmIWZFBhYJzXc_0/images/mission-control.png?fit=max&auto=format&n=XOmIWZFBhYJzXc_0&q=85&s=ac1fe2281515b3cd479a19013b8cf57b" alt="Mission Control orchestration view" width="3392" height="2448" data-path="images/mission-control.png" />
</Frame>

## What Missions do

Factory Missions are structured workflows for taking on large, multi-feature work with Droid. Instead of tackling everything in a single session, you collaborate with Droid upfront to build a plan -- features, milestones, and the skills needed to accomplish each part -- then hand off execution to an orchestration layer that manages the work.

Access Missions with the `/missions` command (also available via `/mission`).

<CardGroup cols={2}>
  <Card title="Collaborative Planning" icon="comments">
    Work with Droid to define features, milestones, and success criteria before any code is written.
  </Card>

  <Card title="Skill-Aware Execution" icon="toolbox">
    Existing skills are leveraged and new specialized skills are developed for each part of the work.
  </Card>

  <Card title="Structured Orchestration" icon="diagram-project">
    Mission Control manages execution across agents, tracking progress through your plan.
  </Card>

  <Card title="Your Config Carries Over" icon="gear">
    MCP integrations, skills, hooks, and custom droids all work inside Missions.
  </Card>
</CardGroup>

## For optimal outcomes

<Warning>
  For the best results, your repository should be at [Agent Readiness](/web/agent-readiness/overview) **Level 4 (Optimized) or above**.

  As a mission works, it runs user-facing QA testing against your application to validate each feature and self-correct as it goes. For this to work in an existing project, your codebase needs an automated, scriptable way to exercise the app the way a user would (for example, a script to stand up or mock all dependencies of the app to simulate all potential user flows). Without it, the mission cannot reliably verify its own work.

  Not there yet? Re-run your readiness evaluation with [`/readiness-report`](/cli/features/readiness-report), then close the gaps with [`/readiness-fix`](/cli/features/readiness-report#remediation-with-readiness-fix).
</Warning>

## How it works

<Steps>
  <Step title="Enter Missions">
    Start by running `/missions` in any Droid session.
  </Step>

  <Step title="Collaborate on the plan">
    Droid interacts with you back and forth to understand your goal. It asks clarifying questions, probes for constraints, and works with you to define what you actually want built. This is a conversation, not a one-shot prompt.
  </Step>

  <Step title="Build features and milestones">
    Based on the conversation, Droid constructs a structured plan: a set of features organized into milestones. Each milestone represents a meaningful checkpoint in the work.
  </Step>

  <Step title="Skills are leveraged or developed">
    Droid pulls in your existing skills where they apply, and develops specialized skills for parts of the work that need them. This means the execution is tailored to your project and workflow, not generic.
  </Step>

  <Step title="Enter Mission Control">
    Once the plan is approved, Droid enters Mission Control -- the Missions orchestration view that manages execution of the plan. You can monitor progress, see which features are being worked on, and intervene when needed.
  </Step>
</Steps>

For details on getting the plan right, see [Planning & Validation](/features/missions/planning). To run and steer an approved mission, see [Running in the CLI](/cli/features/missions/running-cli) or [Running in the Desktop/Web](/web/missions).

## What Missions are good for

We have built and tested Missions across a range of work:

* **Full-stack development** -- Building complete applications with frontend, backend, database, and deployment.
* **Research** -- Deep investigation tasks that require exploring multiple approaches, synthesizing findings, and producing structured output.
* **Brownfield migrations** -- Modernizing existing codebases, swapping frameworks, or restructuring large projects while preserving existing behavior.
* **Ambitious prototypes** -- Product experiments that need to be functional, not just sketched out.

The common thread: work that benefits from upfront planning and structured decomposition rather than ad-hoc prompting.

## Open questions

Missions are early. We are shipping this as a research preview because there are fundamental questions we are still working through:

* **Is parallelization necessary?** Running multiple agents in parallel sounds good in theory, but does it actually produce better results than sequential execution? We are testing this.
* **How do you maximize correctness?** Long-running plans accumulate errors. What validation and correction strategies work best at each stage?
* **Cost vs. quality tradeoffs** -- How aggressive should the orchestrator be? More planning and validation means higher cost but potentially better output. Where is the right balance?

We want your feedback on these. Use Missions, push the workflow hard, and tell us what works and what does not.

## See also

* [Planning & Validation](/features/missions/planning) -- Get the upfront plan right and tune validation frequency
* [Running in the CLI](/cli/features/missions/running-cli) -- Monitor, intervene, and redirect from the terminal
* [Running in the Desktop/Web](/web/missions) -- The visual Mission Control dashboard
* [Troubleshooting](/features/missions/troubleshooting) -- Recover from frozen missions, stuck workers, and blocked milestones
* [Configuration & Reference](/features/missions/reference) -- Headless execution, settings, and enterprise policy
* [Specification Mode](/cli/user-guides/specification-mode) -- For well-scoped tasks that benefit from planning before implementation
* [Implementing Large Features](/cli/user-guides/implementing-large-features) -- Manual workflow for multi-phase projects
* [Custom Droids](/cli/configuration/custom-droids) -- Build specialized subagents that Missions can use
* [Skills](/cli/configuration/skills) -- Create and manage skills that Missions can leverage


Introducing Missions
By Factory - February 26, 2025 - 4 minute read -

Share





An AI system that pursues goals autonomously over multi-day horizons. Describe what you want, approve the scope, and come back to finished work.

Table of Contents









01 Pushing the limits of a single agent


02 Learning and Generalizing as Droid works


03 What it looks like in practice


04 How to use


05 Controls, privacy, and enterprise


06 Open questions


07 Availability

Factory can now see projects through to completion, whether they take six hours or six days. You describe what you want and approve the plan. Droid handles decomposition, execution, and validation.

Try Missions in Factory

"Build me a CRM," "migrate this PHP codebase to TypeScript," "generate test coverage for this undocumented API." Droid breaks the project into features, spawns worker sessions for each one, coordinates handoffs through git, validates at every step, and recovers from failures automatically.

Available in our CLI and IDE extensions. Starting today for Enterprise and Max plan users.

Pushing the limits of a single agent
Mission Control showing a running mission with feature list, progress log, and validation output
Single sessions hit limits. Context windows fill up. Attention degrades over long trajectories. Droid starts forgetting what it already tried, re-reading files, losing track of the bigger picture.

The natural instinct is to run multiple agents in parallel, but coordination is hard. Agents conflict, duplicate work, and drift without structure.

Missions takes a different approach. Instead of fighting the limits of a single agent, we work with them. An orchestrator breaks large projects into milestones, each representing a meaningful checkpoint of progress. Every milestone ends with a validation phase: workers review the accumulated work, run tests, check for regressions, and verify that everything integrates. When validation surfaces issues, the orchestrator creates follow-up work to fix them before moving on.

Within each milestone, the work is broken into features. Each feature gets a fresh worker session with clean context, so no single session has to hold the entire project in its head. When it makes sense, Missions parallelizes within features and during validation, so you get the reliability of sequential execution with the speed of parallel work where coordination overhead is low.

Real missions from production

Legacy Migration
COBOL to Java Spring Boot
Duration
33.8 hrs

Rust Internal Tool
HTTP benchmarking tool from scratch
Duration
22.3 hrs

Systems Debugging
Production memory leak investigation
Duration
24.2 hrs

Greenfield Desktop App
Tauri + React note-taking app with MCP integrations
Duration
30 hrs
Droid has native computer use built in, and we've tuned it specifically for mission workloads. Validation workers launch the application, navigate through flows, check that pages render correctly, and flag visual or functional issues. This means missions can QA applications the way a human would: clicking through the UI, verifying state transitions, catching layout bugs that no test suite would cover. It runs alongside the standard test/lint/build cycle, not as a replacement.

Learning and Generalizing as Droid works
We designed Missions for software development, but they generalize further than we expected. The same system that builds a CRM can write a research paper or train ML models. Goal decomposition, execution, and validation apply to more than code.

Droid does this with a skill-based learning system. When the orchestrator analyzes a new task, it identifies patterns that can be captured as reusable skills. Workers refine and extend the skill library as they work, so Missions gets better at your specific domain the more you use it.

What it looks like in practice
We've been running Missions internally and with early customers since mid-January, with customers ranging from startups to Fortune 500 enterprises, spanning financial services, telecom, and IT services. Here's what the data looks like.

A different kind of workload
Normal Droid sessions are interactive. Fast back-and-forth: the median session lasts about 8 minutes, with 60% finishing within 15 minutes. You ask, the agent responds, you iterate.

Mission sessions are a different distribution entirely. The median mission runs for about 2 hours. 65% run longer than an hour. 37% run longer than four hours. The distribution is nearly flat from 15 minutes out to 24+ hours, which reflects real variance in project complexity rather than the sharp decay of interactive sessions.

Session duration distribution: normal sessions decay sharply (60% under 15 min) while missions are nearly uniform from 15 min to 24+ hours
14% of missions run longer than 24 hours. Some run for days. The longest ran for 16 days. These are persistent, multi-day autonomous workloads that make continuous progress toward a goal.

Missions running longer than 24 hours: broken out from 1-2 days through 2+ weeks, with the longest at 14 days
More reasoning per turn
Missions don't just run longer, they think differently. In a normal session, the agent fires off about 6 messages per minute. In a mission, the rate drops to about 3 messages per minute, but each message carries nearly twice the token weight (19K tokens vs 11K). That lower message rate reflects what missions actually spend time on: running builds, executing test suites, linting, typechecking, and browsing the application under test. Much of a mission's wall-clock time is spent waiting on real-world execution rather than generating tokens.

Session intensity comparison: missions have fewer messages per minute but 2x heavier per message, with 6x more median messages
At the median, a mission consumes 12x more tokens than a normal session. At p99, the gap is 9x. The token burn rate is roughly the same (~45K tokens/min), missions just sustain it for much longer.

Different models for different jobs
A normal Droid session typically only uses one model. Missions use many. The orchestrator, workers, validators, and research agents each have different jobs, and no single model is best at all of them.

As models speciate further, this becomes a structural advantage. Systems locked to one model family will always be constrained by that family's weakest capability. A model-agnostic orchestrator can put the best model in each role regardless of provider, and swap them as the landscape shifts.

Orchestration
Planning, coordination, re-scoping
Opus 4.6
Feature implementation
Code generation, refactoring, testing
Sonnet 4.6 / Opus 4.6
Validation & user testing
Regression detection, integration checks
GPT-5.3-Codex
Research & exploration
Literature review, API exploration, dependency analysis
Kimi K2.5
How to use
Run /enter-mission in any Droid session. Describe what you want built. Droid works with you to scope it: asking clarifying questions, probing for constraints, iterating on the plan. This is a conversation, not a one-shot prompt. The planning phase is where most of the value comes from.

Once you approve the plan, Droid enters Mission Control and begins execution. From there, you're the project manager: monitoring progress, unblocking workers when they get stuck, redirecting when priorities change. Your MCP integrations, skills, hooks, and custom droids all carry over.

Controls, privacy, and enterprise
Missions runs locally or in isolated cloud containers. Git is the source of truth. Every command is classified by risk level, Droid Shield scans for secrets before anything reaches a model, and hooks let you integrate your own security at key points. Every action is logged, and telemetry flows through OpenTelemetry.

Deployment options include cloud-managed, hybrid (LLM traffic terminates inside your network via Azure OpenAI, Bedrock, Vertex, or self-hosted models), and fully airgapped. Org-level policies control allowed models and tools. SSO/SCIM, RBAC, and audit logging are available. Factory maintains SOC 2 Type II, ISO 27001, and ISO 42001 certifications.

How Missions Work
By Theo Luan - April 10, 2026 - 5 minute read -

Share





The architecture behind Missions: why agent context shapes every design decision, how separation of concerns and test-driven development at two levels produce reliable multi-day autonomous work, and how the system actually runs.

Table of Contents







01 Rationale


02 Design Principles


03 The System


04 Breaking down a real mission


05 Looking ahead

Agent sessions work well for focused tasks, but most real projects are too broad and complex for a single context window to hold. A single agent eventually runs into a problem: the more it sees, the less focused and reliable it becomes.

Missions is our system for solving that. It breaks large work into focused units handled by fresh agents with narrowly scoped goals, shared state, and explicit validation.

Rationale
Most of the architecture follows from one core observation: agents are highly reactive to their context.

An agent's trajectory is append-only, so the model's reasoning at any given point is a function of every past thought, observation, and action.
Models seek coherence: they integrate what's in their context into a unified worldview and reason forward from it.
Therefore, they perform best when every previous step in the trajectory urges them toward the next optimal step.
When the context window accumulates information that is irrelevant to - or actively working against - the current goal, performance suffers.
Two failure modes follow from this:

Irrelevant context accumulates. An unfocused or overly broad task means the agent's context grows with information that isn't relevant to what it's doing right now. The broader the scope, the less of the context is pulling the agent toward its next optimal step.
Adversarial context accumulates. An agent that implemented something is worse at objectively evaluating its own work than a fresh, unbiased reviewer. Its prior reasoning creates a bias toward confirming what it already did.
Context Dilution
replay
Trajectory composition over time
Each block is a step in the agent's trajectory. As scope broadens, irrelevant context accumulates and the signal-to-noise ratio drops.
Focused task
Signal
88%
Broad task
Signal
38%
Relevant to current goal
Irrelevant / noise
Self-Evaluation Bias
replay
Prior reasoning anchors evaluation
Each step in its trajectory biases the agent. An implementer accumulates assumptions far from the correct evaluation zone. A fresh validator's exploration converges into it.
evaluate only
correct evaluation
reading auth module output
testing login with valid creds
testing expired token flow
checking rate limit behavior
tracing session persistence
found missing error on malformed JWT
found race condition in refresh
Implementation steps
Evaluation steps
Correct evaluation zone
Agent
Implications
It's not enough to simply split up work. Each agent's goal must be focused, and its trajectory directionally consistent. In every run, we must avoid accumulating context that is:

not useful to the agent's current task
not aligned with the agent's incentive, or our ideal outcome for its run
Design Principles
Separation of concerns and incentives
Each role has a single goal, and the system is structured so that nothing in an agent's trajectory pulls it away from that goal.

The orchestrator plans and decomposes an approach to the user's goal, and steers execution to completion, passing all validation gates. It avoids accumulating overly granular context, delegating all investigation and implementation to subagents and workers. It doesn't drive validation directly - the system injects validators at milestones to surface gaps.
Workers complete well-specified features with clear success criteria. They iterate until they believe the work is correct, then hand it off. But the final judgment on correctness is not their call. An independent validator decides that.
Validators evaluate completed work for correctness and completeness, surfacing bugs and gaps. They don't implement fixes - they surface issues to the orchestrator, which creates fix features that future workers implement.
Test-driven development at two levels
The same principle operates at two scales.

Each worker writes tests before code, so the tests reflect intended behavior rather than implementation details.
At the mission level, the orchestrator defines correctness first - creating a validation contract, a set of behavioral assertions that define success, before defining any features.
This ordering matters. When creating the validation contract, the orchestrator draws from its understanding of requirements. If it had created the features first, the contract would be influenced by the implementation it had already planned.

These assertions are later verified by fresh agents that exercise the system as a black box - using it the way a real user would - rather than inspecting the code that implements them.

Externalized state
No single agent needs to hold the complete picture in its context at once. The full state is distributed across shared artifacts: the validation contract, the feature list, research notes, operational guidelines, and an evolving knowledge base.

Each agent reads what's relevant to its current job. Even the orchestrator delegates deep investigation to subagents to avoid consuming every detail itself.

Model specialization
Different models have different strengths - reasoning, discipline, creativity, thoroughness, speed, cost. No single model is best at everything.

Once roles are cleanly separated, model choice becomes local to each role: broad planning and judgment for the orchestrator, reliable execution and cost efficiency for workers, thoroughness and skepticism for validators.

The System
With those principles in mind, here's how a mission actually runs.

A user describes what they want built. The orchestrator investigates and asks clarifying questions until the requirements are unambiguous.

Then it writes the validation contract - a finite checklist of testable behavioral assertions that define completion and correctness for the mission.

From there, it decomposes the work into features, where each feature is a bounded piece of implementation that claims which assertions it will fulfill. Features are grouped into milestones, each of which encompasses a logical unit of functionality.

Finally, it creates shared state files - boundaries and procedures for its workers that enforce optimal structure and behavior, as well as a library that will accumulate knowledge over the mission's duration.

validation-contract.md
features.json
services.yaml
AGENTS.md
markdown
### VAL-AUTH-001: Successful login
A user with valid credentials submits the login form
and is redirected to the dashboard.
Tool: agent-browser
Evidence: screenshot, network(POST /api/auth/login -> 200)
 
### VAL-CROSS-001: Auth gates pricing
A guest user sees "Sign in for pricing" on the catalog.
After logging in, real prices are shown.
Tool: agent-browser
Evidence: screenshot(guest-view), screenshot(authed-view)
...
A programmatic runner takes the feature list and spawns a worker for each feature in order. Each worker starts with a fresh context, receives its feature spec, writes tests first, then implements.

Once all features within a milestone are complete, the runner triggers validation using fresh agents.

Scrutiny validators review each worker's implementation and trajectory for quality and correctness, and encode relevant knowledge updates into shared state.
User-testing validators exercise the system as a black box - using it the way a real user would - and verify behavior against the validation contract.
After validation, the orchestrator reviews what workers and validators flagged. It creates fix features targeted at actionable gaps, which get executed before the milestone re-validates. This loop repeats until milestone validation passes.

If implementation or validation is blocked, the orchestrator halts the mission and hands control back to the user.

Breaking down a real mission
A single mission produced a Slack clone - workspace auth, channels and threads, real-time messaging with reactions and mentions, file uploads, search, and presence and notifications.

Time
Total runtime — 16.5 h
Orchestration
0.38 h · 2.3%
Implementation
9.98 h · 60.5%
Validation
6.14 h · 37.2%
Milestone waterfall
impl
val
0h2h4h6h8h10h12h14h16h
Foundation
Channels & messaging
Conversations
Interactions
Rich features
Real-time polish
Agent Runs
185 total runs
subagents
3
6
9
12
0h2h4h6h8h10h12h14h16h
Orchestration
1 · 12 subagents
Workers
63
Validators
27 · 82 subagents
Tokens
778.5M total tokens
By type
Input
30.3M
Cache read
744.9M
Output
3.4M
By role
Orchestration
29.2M
Implementation
485.5M
Validation
263.8M
Code
38.8k lines — 52.5% tests
Code split
Source
18.5k lines
Tests
20.4k lines
Statement coverage
Covered
89.25%
Uncovered
10.75%
This mission progressed through a consistent implementation-validation cadence across six milestones, with validation accounting for 37.2% of total runtime.

It generated 38.8k lines of code (52.5% of those lines tests) with 89.25% statement coverage.

Reliability Loop
Cumulative milestone passes by validation round
Round 1
0/6 milestones passed
Round 2
1/6 milestones passed
Round 3
2/6 milestones passed
Round 4
6/6 milestones passed
Passes are recorded on the final round of each milestone.
Fix feature ratio
21 fix features / 61 implementation features (34.4%)
Original
40 features
Fix
21 features
Issues surfaced by validators
81 total issues
Blocking
65 issues
Non-blocking
11 issues
Suggestion
5 issues
Median trajectory length
Implementation
51 turns · p90 123
Validation
30 turns · p90 37
Every milestone converged in 2-4 validation rounds. That produced a steady correction loop: validators surfaced 81 issues, and the orchestrator generated 21 targeted fix features (34.4% of implementation work) to close them.

Trajectories also stayed bounded throughout execution, with median run lengths of 51 assistant turns for implementation and 30 for validation.

Looking ahead
Missions is our first version of a system that closes the software development loop.

As models get better at reasoning, planning, execution, and computer use, each improvement compounds through the architecture: better planners produce tighter specs, better workers make fewer mistakes, and better validators can judge correctness more reliably across a wider range of surfaces. As models get faster and cheaper, the loop gets tighter - more validation rounds become practical. More ambitious missions become viable for more teams and codebases.

Missions is available today. Run /missions in any Droid session to start one.