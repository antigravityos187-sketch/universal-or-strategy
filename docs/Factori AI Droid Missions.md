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

## The planning phase matters most

The biggest value we have found in Missions is in the planning phase. Getting the upfront plan right -- the features, the ordering, the milestones, the skills involved -- is what determines whether the execution succeeds. Droid will push back, ask questions, and iterate with you until the plan is solid.

This is intentional. A well-scoped plan with clear milestones produces dramatically better results than jumping straight into execution on a vague goal.

### Validation

* **Milestones** define validation frequency. Validation workers run at the end of each milestone, verifying its work. For simple projects, one milestone is often enough; for longer or complex projects, more frequent milestone validation helps keep the foundation stable as work scales.

For smaller, straightforward projects, a single milestone is often enough. For larger or longer-running projects, more granular milestones can prevent drift and reduce expensive rework later.

### Estimating cost and duration

As a rough planning heuristic, mission duration and cost scale with the number of worker runs:

* **Feature workers:** roughly one run per feature
* **Validator workers:** roughly one run per milestone

So an initial estimate is approximately:

`total runs ≈ #features + 2 * #milestones`

In practice, this is a floor rather than a ceiling. Validation may surface issues that require follow-up work, and the orchestrator can create additional fix features during execution.

## What Missions are good for

We have built and tested Missions across a range of work:

* **Full-stack development** -- Building complete applications with frontend, backend, database, and deployment.
* **Research** -- Deep investigation tasks that require exploring multiple approaches, synthesizing findings, and producing structured output.
* **Brownfield migrations** -- Modernizing existing codebases, swapping frameworks, or restructuring large projects while preserving existing behavior.
* **Ambitious prototypes** -- Product experiments that need to be functional, not just sketched out.

The common thread: work that benefits from upfront planning and structured decomposition rather than ad-hoc prompting.

## Working with Mission Control

Once the plan is approved, Droid enters Mission Control -- the orchestration view that manages execution. From here you can track progress across features and milestones, see which agents are working on what, and intervene when things need adjustment.

### Intervening and redirecting

Missions are not fire-and-forget. The orchestrator is an agent, and you can talk to it. The most effective way to use Missions is to treat yourself as the project manager -- monitoring progress, unblocking workers, and redirecting when the plan needs to change.

<AccordionGroup>
  <Accordion title="The mission freezes or stops making progress">
    If the mission appears stuck and nothing is happening, pause the orchestrator and tell it what you are seeing. Be direct: explain that the mission appears frozen or broken, describe what the last visible activity was, and ask it to recover. The orchestrator can re-assess the state and pick back up.

    **Example:** *"The mission seems frozen -- the last worker finished 10 minutes ago and nothing new has started. Re-assess and continue."*
  </Accordion>

  <Accordion title="A worker is taking too long on a single item">
    If one worker is spinning on a task for too long without making meaningful progress, you do not need to wait for it to finish. Pause the orchestrator and tell it to mark the current item as complete and move on. You can always come back to that item later, or handle it manually.

    **Example:** *"The worker on the auth integration has been stuck for 20 minutes. Mark it as complete and move to the next feature."*
  </Accordion>

  <Accordion title="The mission is stuck on a milestone">
    Sometimes the orchestrator hits a milestone that has become blocked -- maybe an earlier assumption was wrong, or a dependency is missing. When this happens, ask the orchestrator to re-assess the remaining work and figure out why it has become blocked. It can re-plan around the obstacle, reorder features, or adjust the milestone scope.

    **Example:** *"We are stuck on Milestone 3. Re-assess the remaining work and tell me what is blocking progress."*
  </Accordion>

  <Accordion title="You want to change direction mid-mission">
    If you realize the plan needs to change -- a feature should be dropped, a new requirement has come in, or the approach is wrong -- pause and tell the orchestrator. It can update the plan, re-scope milestones, and continue from the new direction.

    **Example:** *"Drop the email notification feature and add Slack integration instead. Re-plan the remaining milestones."*
  </Accordion>
</AccordionGroup>

### A new kind of debugging

The skillset for working with Missions looks less like traditional debugging and more like **project management of agents**. You are not stepping through code line by line -- you are monitoring a team of workers, unblocking them when they get stuck, redirecting them when priorities change, and making judgment calls about when to push through versus when to re-plan.

This is a meaningfully different way of working with AI. The core skill is knowing when and how to intervene, not writing the code yourself.

## Configuration inheritance

Missions inherit your existing Droid configuration:

* **MCP integrations** -- Workers can use your connected tools (Linear, Sentry, Notion, etc.)
* **Custom skills** -- Your existing skills are available and new ones can be developed during planning.
* **Hooks** -- Lifecycle hooks fire during mission execution.
* **Custom droids** -- Subagents configured in your project are available to workers.
* **AGENTS.md** -- Workers follow your project conventions and coding standards.

## Headless mission execution

Missions can also run non-interactively via `droid exec --mission`. This is useful for CI, scheduled jobs, and any environment where you want the orchestrator to plan and execute without a live TUI.

```bash theme={null}
droid exec --mission -f mission.md
```

You can override the models and reasoning effort used by the orchestrator's worker and validator agents:

```bash theme={null}
droid exec --mission \
  --worker-model claude-sonnet-4-6 \
  --worker-reasoning-effort medium \
  --validator-model claude-opus-4-7 \
  --validator-reasoning-effort high \
  -f mission.md
```

| Flag                                   | Description                                                   |
| -------------------------------------- | ------------------------------------------------------------- |
| `--mission`                            | Run `droid exec` in mission mode (multi-agent orchestration). |
| `--worker-model <id>`                  | Model used for mission worker agents.                         |
| `--worker-reasoning-effort <level>`    | Reasoning effort for mission worker agents.                   |
| `--validator-model <id>`               | Model used for mission validator agents.                      |
| `--validator-reasoning-effort <level>` | Reasoning effort for mission validator agents.                |

See [`droid exec`](/cli/droid-exec/overview) for the full headless reference.

## Configuration

Missions are tuned through the `missionModelSettings` object and a few top-level keys. Set these in your global or project [settings](/cli/configuration/settings) file.

| Setting                                                | Description                                                                    |
| ------------------------------------------------------ | ------------------------------------------------------------------------------ |
| `missionModelSettings.workerModel`                     | Default model used by mission worker subagents.                                |
| `missionModelSettings.workerReasoningEffort`           | Reasoning effort for mission workers (`off`, `none`, `low`, `medium`, `high`). |
| `missionModelSettings.validationWorkerModel`           | Model used by validation workers (scrutiny and user-testing).                  |
| `missionModelSettings.validationWorkerReasoningEffort` | Reasoning effort for validation workers.                                       |
| `missionModelSettings.skipScrutiny`                    | Skip scrutiny validation milestones during missions.                           |
| `missionModelSettings.skipUserTesting`                 | Skip user-testing validation milestones during missions.                       |
| `missionOrchestratorModel`                             | Model used by the mission orchestrator.                                        |
| `missionOrchestratorReasoningEffort`                   | Reasoning effort for the mission orchestrator.                                 |
| `keepSystemAwakeDuringMissions`                        | Prevent the OS from sleeping while a mission is running. Defaults to `true`.   |

<Tip>
  Pairing a strong orchestrator model with a faster worker model is a common cost-quality tradeoff: planning and validation benefit most from extra reasoning, while routine worker tasks can use a lighter model.
</Tip>

### Enterprise: restricting mission access

Organizations can restrict who is allowed to launch missions through the `missionPolicy` org-level setting:

```json theme={null}
{
  "missionPolicy": {
    "restrictedAccess": true,
    "allowedUserIds": ["user_123", "user_456"]
  }
}
```

When `restrictedAccess` is `true`, only members listed in `allowedUserIds` can start new missions. See the [settings reference](/cli/configuration/settings) for the full enterprise policy surface.

## Open questions

Missions are early. We are shipping this as a research preview because there are fundamental questions we are still working through:

* **Is parallelization necessary?** Running multiple agents in parallel sounds good in theory, but does it actually produce better results than sequential execution? We are testing this.
* **How do you maximize correctness?** Long-running plans accumulate errors. What validation and correction strategies work best at each stage?
* **Cost vs. quality tradeoffs** -- How aggressive should the orchestrator be? More planning and validation means higher cost but potentially better output. Where is the right balance?

We want your feedback on these. Use Missions, push the workflow hard, and tell us what works and what does not.

## See also

* [Specification Mode](/cli/user-guides/specification-mode) -- For well-scoped tasks that benefit from planning before implementation
* [Implementing Large Features](/cli/user-guides/implementing-large-features) -- Manual workflow for multi-phase projects
* [Custom Droids](/cli/configuration/custom-droids) -- Build specialized subagents that Missions can use
* [Skills](/cli/configuration/skills) -- Create and manage skills that Missions can leverage


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

Open questions
Missions is early. It handles complex multi-day projects, but there are fundamental questions we're still working through.

How much parallelization actually helps. Serial execution with targeted parallelization has worked better than broad parallelism. But the right balance probably depends on the project. Where does coordination overhead outweigh the speed gains?

Correctness over long horizons. Long-running plans accumulate errors. Milestone validation catches most, but the orchestrator still scopes too broadly sometimes, and workers get stuck on edge cases a human would navigate easily.

Worker scope. Narrow scope keeps workers focused but increases overall cost and introduces more coordination overhead. Broad scope maintains continuity within features, but stretches each agent's attention thinner.

Recursive management depth. The orchestrator manages workers directly. Some tasks might benefit from sub-orchestrators managing their own workers. One layer works for most projects. Two might help for larger ones. Three starts to feel like a bureaucracy.