> ## Documentation Index
> Fetch the complete documentation index at: https://docs.traycer.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# Epic Mode

> Collaborative artifact management through structured workflows. Perfect for managing specs and tickets with shared boards, real-time collaboration, and AI-guided development processes.

Epic Mode is designed to solve one of the biggest challenges in AI-assisted development: **preserving human intent** from initial idea to implementation. When building with AI agents, critical context often lives scattered across chat messages or exists only in your head - the "why" behind decisions, constraints, edge cases, and invisible rules. This is how drift happens: agents aren't trying to be wrong, they're just filling in gaps.

Epic Mode captures this intent through a **system of specs** - not one giant document that becomes outdated, but focused mini-specs that each address a specific aspect of your project. Combined with structured workflows, actionable tickets, and verification at every stage, Epic Mode keeps your development process aligned with your original intent.

<Frame>
  <img src="https://mintcdn.com/traycerai/NABm2nVgOe92_o_v/images/epic-view.png?fit=max&auto=format&n=NABm2nVgOe92_o_v&q=85&s=b7357c0f03623e8c110107761b7640d4" alt="Epic Mode board view showing specs and tickets" width="3164" height="1882" data-path="images/epic-view.png" />
</Frame>

## Understanding Artifacts

Epic Mode helps you create and manage **artifacts** - structured documents that form a system of interconnected specs and tickets. This system captures your development process from requirements to implementation. Artifacts come in two types:

### Specifications (Specs)

Specs are focused, high-level documents that capture requirements, design decisions, and technical planning. They provide the "why" and "what" of your project. Rather than one giant spec, Epic Mode favors **mini-specs** - each tightly scoped to a specific aspect, making them easier to maintain and update as your project evolves.

**Common spec types:**

* **PRD (Product Requirements Document)**: Defines the problem, who's affected, and the desired outcome at a product level
* **Tech Doc**: Outlines architecture, technical approach, and implementation strategy
* **Design Spec**: Documents user flows, UX decisions, and interaction patterns
* **API Spec**: Defines API contracts, endpoints, and integration requirements

Specs are living documents - they evolve as your understanding deepens and requirements change. When something changes, you update the relevant mini-spec instead of rewriting everything.

<Frame>
  <iframe className="w-full aspect-video rounded-xl" src="https://www.youtube.com/embed/Yh4Rn0mPPaU" title="Creating a spec" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen />
</Frame>

### Tickets

Tickets are actionable work items that break down specs into concrete implementation tasks. Each ticket represents a focused unit of work that can be independently implemented.

**Ticket characteristics:**

* Contains clear acceptance criteria
* Tracks status: Todo → In Progress → Done
* Can be assigned to specific collaborators or left Unassigned
* Can be handed off to coding agents for implementation

Together, specs and tickets form a complete development workflow: specs capture the strategic thinking, while tickets drive the tactical execution.

<Frame>
  <iframe className="w-full aspect-video rounded-xl" src="https://www.youtube.com/embed/3FPpIJc3CHQ" title="Creating a ticket" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen />
</Frame>

<Info>
  **Full Context Awareness:** All specs and tickets within an epic are automatically in the LLM's context. When you're discussing a ticket or spec, the AI has full awareness of all related artifacts, previous decisions, and conversations - enabling coherent, context-aware development throughout your entire epic.
</Info>

## How It Works

<Steps>
  <Step title="1. Choose a Workflow">
    Select a workflow that will guide you through the development process with structured commands.

    Learn more about [workflows](/tasks/workflows).
  </Step>

  <Step title="2. Start with Requirements">
    Provide your requirements, problem statement, or goals to the workflow's entrypoint command.

    The AI begins processing according to the workflow's instructions.
  </Step>

  <Step title="3. Follow the Workflow">
    Progress through the workflow's commands. Epic Mode emphasizes **dialogue and elicitation** - the AI doesn't just generate documents, it actively asks pointed questions to surface constraints, edge cases, and the "invisible rules" behind your requirements.

    During this process, the AI will:

    * Ask clarifying questions to understand your intent deeply
    * Help you make explicit decisions instead of leaving ambiguity
    * Propose and create specification documents
    * Generate actionable tickets for implementation
    * Guide you through additional workflow-specific steps
  </Step>

  <Step title="4. Handoff to Implementation">
    Once you have the specs and tickets you need, select them and hand off for implementation. Verification is built into Traycer's implementation modes to continuously validate that execution matches your captured intent.

    See [Selection and Handoff](#selection-and-handoff) for details on the available options.
  </Step>
</Steps>

## Executions

Executions provide complete visibility into every agent handoff within your Epic. Each execution represents a discrete unit of work sent to a coding agent, capturing the full lifecycle from handoff to completion.

<Frame>
  <img src="https://assets.traycer.ai/executions_view.png" alt="Executions tracking in Epic view" />
</Frame>

### What Gets Tracked

Every execution captures:

* **Plans**: Implementation plans generated (if any) during execution
* **Verification Comments**: Post-execution verification results with any review comments or issues found
* **Commit**: Git commit created after successful execution
* **Status**: Current state of the execution (running, completed, verification pending, etc.)

### Viewing Executions

All executions are visible directly in the Epic view:

* **Execution History**: See every handoff that's occurred, both manual and automated
* **Execution Details**: Click into any execution to see the full context, what was handed off, and verification results
* **Real-time Updates**: Watch executions progress as agents work

This gives you a complete audit trail of how your Epic was implemented, making it easy to understand what changed, when, and why.

## Smart YOLO

Smart YOLO is Traycer's intelligent orchestrator that can execute entire Epics end-to-end with minimal human intervention. Simply use the `/execute` command or tell Traycer to execute your specs and tickets, and Smart YOLO takes over - automatically coordinating execution, verification, and iterative refinement until your Epic is complete.

**What makes it "smart":** Unlike traditional automation, Smart YOLO dynamically adjusts its execution strategy at runtime. It analyzes each spec and ticket, then intelligently configures every aspect of execution - from selecting the right agents and templates to determining whether to skip planning, which verification levels to address, appropriate timeouts, and whether to auto-commit. Every setting is optimized for the specific task at hand.

Smart YOLO is ideal when you have well-defined specs and tickets ready for implementation and want intelligent automated execution with minimal manual coordination.

For full documentation on Smart YOLO features and all dynamic configuration capabilities, see [YOLO Mode](/tasks/yolo-mode#smart-yolo-for-epic-mode).

## Managing Specs and Tickets

Epic mode provides comprehensive artifact management capabilities. All specs and tickets are organized in the **Documents** panel on the side, where you can view, select, and manage them.

### Specs

* **Create**: Generate specs through AI-assisted conversations or use the **+ Add Spec** button
* **View & Edit**: Select any spec from the list to view and edit its contents
* **Organize**: All specs are grouped in the SPECS section of the Documents panel

### Tickets

* **Create**: Break down work into actionable tickets or use the **+ Add Ticket** button
* **View & Edit**: Select any ticket from the list to view and edit its contents
* **Status Tracking**: Track ticket status from Todo → In Progress → Done
* **Assignment**: Assign tickets to specific collaborators or leave them Unassigned
* **Organize**: All tickets are grouped in the TICKETS section of the Documents panel

**Assigning tickets:** Once you've shared your Epic board with team members, you can assign tickets directly from the ticket view. Learn more about [ticket assignment and collaboration](/tasks/collaboration#ticket-assignment).

### Selection and Handoff

Use the **Select** button to enter selection mode and choose specific specs and tickets. Once selected, you can:

* **Refer in Chat**: Reference selected artifacts in conversation
* **Execute in Phases**: Hand off to Phases mode for implementation
* **Handoff To**: Send to your preferred coding agent

## Collaboration

Epic boards can be shared with your team so everyone works from the same set of specs and tickets. Share boards with teammates, assign tickets to specific collaborators, and work together in real time with live updates and presence indicators.

**Key collaboration features:**

* **Invite by email or GitHub handle** - Add collaborators individually or share with your entire organization
* **Assign tickets** - Distribute work across team members directly from the ticket view
* **Real-time editing** - Multiple people can edit specs and tickets simultaneously with live updates
* **Access control** - Set Editor or Viewer permissions based on each person's role

Click the share button next to `Open Board` in the top bar to get started.

📖 For detailed guides on inviting team members, ticket assignment, access levels, best practices, and use cases, see the [Collaboration documentation](/tasks/collaboration).


> ## Documentation Index
> Fetch the complete documentation index at: https://docs.traycer.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# Phases Mode

> Structured, multi-phase development for complex projects. Break goals into iterative phases with validation between steps.

<Frame>
  <iframe className="w-full aspect-video rounded-xl" src="https://www.youtube.com/embed/1tPbWsT5npU?rel=0&cc_load_policy=1&cc_lang_pref=en&start=67" title="Phases mode" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen />
</Frame>

<Steps>
  <Step title="1. User Query">
    State your goal, expected outcome, and constraints.

    <Accordion title="What context can I provide?">
      Optional context:

      * **Files**: Source files, config files, documentation, test files.
      * **Folders**: Component directories, feature folders, asset directories.
      * **Images**: UI mockups, error screenshots.
      * **Git**:
        * **Diff against uncommitted changes**: Changes that haven't been committed yet.
        * **Diff against 'main'**: Changes compared to the main/default branch.
        * **Diff against branch**: Changes compared to a specific branch of your choice.
        * **Diff against commit**: Changes compared to a specific commit.
    </Accordion>
  </Step>

  <Step title="2. Intent Clarification (if needed)">
    Traycer asks strategic questions to refine scope:

    * Business goals and user flows.
    * Architecture and integration needs.
    * Performance, security, and scalability requirements.
  </Step>

  <Step title="3. Phase Generation">
    Traycer structures work into manageable phases:

    * **Phase identification**: Clear milestones and outcomes.
    * **Sequential breakdown**: Logical progression from start to finish.
    * **Scope definition**: Well-defined boundaries for each phase.

    Learn more about [managing phases](/tasks/phases#managing-phases).
  </Step>

  <Step title="4. Phase Planning">
    Traycer creates a detailed plan for each phase:

    * **Objectives** and deliverables.
    * **File changes** with exact edits.
    * **Architecture** and approach.

    Learn more about [plan](/tasks/plan) for each phase.
  </Step>

  <Step title="5. Hand off to Agent">
    Execute the generated plan with your AI coding assistant. See [supported agents](/integrations/agents).
  </Step>

  <Step title="6. Verification with Traycer">
    Validate each phase before moving on:

    * Compares agent's implementation against your original plan to ensure requirements.
    * Categorizes verification review comments by severity - Critical, Major, Minor, Outdated.

    Learn more about [verification](/tasks/verification).
  </Step>

  <Step title="7. Next Phase">
    Advance with preserved context:

    * Carry forward decisions and mappings.
    * Clear progress tracking.
    * Plans adapt based on learnings.
  </Step>
</Steps>

## Automating phases with YOLO Mode

<Card title="YOLO Mode" icon="robot" href="/tasks/yolo-mode">
  **Fully automated workflow.** Configure once and let Traycer automatically orchestrate planning, coding, and verification across all phases without manual handoff clicks.
</Card>

## Managing phases

After Traycer creates your initial phases, you have full control over the phase structure and can modify it as your project evolves.

### Phase selection mode

You can select multiple phases at once and refer to them in chat or merge them into a single phase using AI.

<Frame>
  <iframe className="w-full aspect-video rounded-xl" src="https://www.youtube.com/embed/FDX7CNIp0ek?rel=0&cc_load_policy=1&cc_lang_pref=en&start=67" title="Phase selection mode" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen />
</Frame>

### Adding more phases

<Frame>
  <img src="https://mintcdn.com/traycerai/nzzd5emSQefoVR6u/images/add-phase.gif?s=0c90f999f7843d53132708655153999a" alt="Phases add" width="800" height="450" data-path="images/add-phase.gif" />
</Frame>

You can add additional phases at any point in your project:

* **Insert new phases**: Add phases between existing ones or at the end.
* **Address new requirements**: Add phases for features or changes discovered during development.
* **Refinement phases**: Add phases for optimization, testing, or documentation.

### Re-arranging phase order

<Frame>
  <img src="https://mintcdn.com/traycerai/nzzd5emSQefoVR6u/images/phase-rearrange.gif?s=81eb60f6f5307ba5d26dfd5a27092235" alt="Phases rearrange" width="1280" height="720" data-path="images/phase-rearrange.gif" />
</Frame>

Traycer allows you to re-arrange the order of phases to optimize your development process:

* **Drag and drop**: Simply drag phases to reorder them in the sequence.
* **Flexible sequencing**: Change the order based on new insights or changing priorities.
> ## Documentation Index
> Fetch the complete documentation index at: https://docs.traycer.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# Workflows

> Create and manage structured development processes. Define custom workflows with commands that guide your team through your unique methodology.

Workflows are structured processes that guide you through development tasks using a series of commands. Each workflow represents a methodology or approach to software development, from initial requirements to implementation.

Traycer comes with default workflows like the **Traycer Agile Workflow**, and you can create custom workflows tailored to your team's unique processes.

## What is a Workflow?

A workflow is a collection of command files that guide you through a development process. Each workflow consists of:

* **Name & Description**: Identify the workflow and its purpose
* **Entrypoint Command**: The starting point (default: `trigger_workflow`, customizable)
* **Command Files**: Additional steps in your process

Workflows are used within [Epic Mode](/tasks/epic) to structure your development process.

<Frame>
  <img src="https://mintcdn.com/traycerai/WJ5d37qSdEF0Z5Pz/images/workflows.png?fit=max&auto=format&n=WJ5d37qSdEF0Z5Pz&q=85&s=997b7c318bc0e64ceb604b71f68ede72" alt="Workflow Structure" width="1536" height="1024" data-path="images/workflows.png" />
</Frame>

### Workflow Structure

Each workflow has:

* **Workflow Metadata**: Name and description
* **One Entrypoint**: The command file that starts the workflow
* **Multiple Commands**: Each command is a file with instructions for the AI

**Important**: The file name and command name are the same thing.

## Using Workflows

### Triggering a Workflow

To start a workflow in Epic Mode:

1. **Select a workflow** from the dropdown when creating or working in an Epic. Available options include:
   * Your custom workflows
   * **Traycer Agile Workflow** (default)
   * **No Workflow** - Run without a structured workflow

2. **Choose a command** by typing `/` in the chat input to see all available commands from your selected workflow:
   * `/trigger_workflow` (or your custom entrypoint) - Start the workflow
   * `/command-name` - Any other command in the workflow

3. **Provide context** after selecting the command - the slash becomes regular text after selection, allowing you to add your requirements or arguments.

**Note**: Only one command per query. After selecting a command with `/`, the slash character will be treated as regular text.

## Creating Custom Workflows

You can create custom workflows tailored to your team's methodology using the **+ Add Workflow** button.

### Creating a Workflow

1. Click **+ Add Workflow** in the Workflows panel
2. Enter a **Name** and **Description** for your workflow
3. An entrypoint command file is automatically created (default: `trigger_workflow`)

<Frame>
  <img src="https://mintcdn.com/traycerai/WJ5d37qSdEF0Z5Pz/images/add-new-workflow.gif?s=14ff0c4e779facc2eb08d3bf3ef45604" alt="Adding new workflow" width="800" height="502" data-path="images/add-new-workflow.gif" />
</Frame>

### Adding Commands

To add commands to your workflow:

1. Click **+ Add Command** in the Workflow Commands panel
2. Give your command a descriptive name (this becomes the file name)
3. Configure the command's properties and content

### Command Configuration

Each command has three key components:

#### 1. Description

A brief explanation of what this command does - shown to users when they type `/` to select commands.

#### 2. Argument Hints

Optional hints that guide what information should be passed when calling this command. Click **Add new hint** to add hints.

See [Using Arguments](#using-arguments) for details on how to reference arguments in your command instructions.

#### 3. Next Steps

Define which commands can follow this one. Click **Add next step** to select commands from your workflow.

See [Multi-Path Workflows](#multi-path-workflows) for details on how the AI suggests next steps.

### Writing Command Instructions

Use the markdown editor to write instructions for the AI. You can structure your instructions however makes sense for your workflow, and reference arguments using `$1`, `$2`, etc. where needed.

## Managing Workflows

### Viewing Workflows

Workflows are organized in the Workflows panel on the left side:

* **Default Workflows**: Marked as "Default (Read-only)" - these are bundled with Traycer and cannot be edited
* **Custom Workflows**: Your own workflows that you can fully edit and customize

Access workflows through:

* The Workflows panel in the sidebar
* Workflow selector when creating a new Epic
* Click the menu (three dots) and select "Manage workflow" to open the workflow viewer

<Frame>
  <img src="https://mintcdn.com/traycerai/WJ5d37qSdEF0Z5Pz/images/view-workflow.gif?s=855109090ea2faa7b60ab9a1002e50aa" alt="Viewing workflow" width="800" height="502" data-path="images/view-workflow.gif" />
</Frame>

### Cloning Workflows

**Default Workflows** are read-only but can be cloned. Click **Clone to Edit** to create an editable copy that you can customize.

<Frame>
  <img src="https://mintcdn.com/traycerai/WJ5d37qSdEF0Z5Pz/images/clone-workflow.gif?s=4ddd2d1314e2753b8e7754af6909d73e" alt="Cloning workflow" width="800" height="502" data-path="images/clone-workflow.gif" />
</Frame>

### Editing Workflows

**Custom Workflows** are fully editable:

1. Select your workflow from the Workflows panel
2. Click on any command to edit its content
3. Modify command metadata (description, argument hints, next steps)
4. Changes save automatically

### Managing Commands

| Action             | Description                                                                                    |
| ------------------ | ---------------------------------------------------------------------------------------------- |
| **Add Command**    | Create a new command file in your workflow using **+ Add Command**                             |
| **Edit Command**   | Click any command to modify its content and metadata (description, argument hints, next steps) |
| **Delete Command** | Remove a command (entrypoint cannot be deleted)                                                |

### Deleting Workflows

Custom workflows can be deleted from the Workflows panel. Default workflows cannot be deleted.

## Integration with Epic Mode

Workflows are the backbone of [Epic Mode](/tasks/epic):

* **Structured Guidance**: Workflows guide the development process
* **Context Preservation**: Decisions flow between commands
* **Artifact Creation**: Commands generate specs and tickets
* **Flexible Execution**: Follow or adapt workflow as needed

## Advanced Workflow Features

### Using Arguments

Commands can accept arguments that provide context when they're called. Here's how to set them up:

**1. Configure Argument Hints**

When editing a command, add argument hints that describe what information users should provide. For example:

* "Feature name or requirement"
* "Technology or approach"

**2. Reference Arguments in Command File**

In your command instructions, use `$1`, `$2`, `$3`, etc. to reference the arguments:

```markdown theme={null}
<processing_user_request>
The user wants to implement: $1
Using technology: $2
</processing_user_request>
```

**3. Call the Command with Arguments**

When users call the command, they provide the actual values:

```
/create-feature User authentication OAuth2 and JWT
```

The AI automatically maps the arguments:

| Reference | Argument Hint                 | Actual Value        |
| --------- | ----------------------------- | ------------------- |
| `$1`      | "Feature name or requirement" | User authentication |
| `$2`      | "Technology or approach"      | OAuth2 and JWT      |

### Multi-Path Workflows

Use Next Steps to create workflows with alternative paths. The AI will suggest or let the user choose from the configured next commands based on the context.

**Example:** A command might have multiple possible next steps:

* `design-review` - for UI-heavy features
* `tech-plan` - for backend features
* `spike-investigation` - for uncertain requirements

The AI will recommend the most appropriate next step or present options to the user.

### Agent Modes

Workflows support different **agent modes** that can be selected per command. Each mode is optimized for specific use cases with dedicated models, reasoning configurations, and system prompts.

<Frame>
  <img src="https://assets.traycer.ai/agent_modes_selection.png" alt="Agent mode selection in workflows" />
</Frame>

#### Available Modes

**Planner Mode**

* Optimized for strategic thinking and planning tasks
* Uses extended reasoning capabilities for complex analysis
* Ideal for commands that create specs, design systems, or break down requirements
* Custom system prompts focused on thorough exploration and documentation

**Reviewer Mode**

* Optimized for evaluation and quality assessment
* Configured for detailed review and verification workflows
* Ideal for commands that validate, critique, or provide feedback on artifacts
* System prompts emphasize thoroughness, edge cases, and improvement suggestions

#### Using Agent Modes

When creating or editing a command, you can specify which agent mode should be used when that command executes:

1. Select the command in your workflow
2. Choose the appropriate agent mode from the dropdown
3. The selected mode will be used whenever that command runs

This gives you fine-grained control over how each step in your workflow behaves, ensuring the right "thinking style" is applied at each stage of your development process.

## Traycer Agile Workflow

The default workflow guides you through feature development with a collaborative, spec-driven approach.

### Workflow Commands

<Steps>
  <Step title="trigger_workflow">
    **Purpose**: Initial requirements gathering and clarification

    * Discuss user's request and goals
    * Ask clarifying questions
    * Build shared understanding
    * No assumptions - alignment first

    **Next Steps**: `epic-brief` or `core-flows`
  </Step>

  <Step title="epic-brief">
    **Purpose**: Define problem and context collaboratively

    * Capture who's affected and current pain points
    * Document the problem at a product level
    * Create concise Epic Brief spec (under 50 lines)
    * No UI specifics or technical design yet

    **Next Steps**: `core-flows`
  </Step>

  <Step title="core-flows">
    **Purpose**: Map out user flows and interactions

    * Explore current product flows
    * Design UX decisions (information hierarchy, user journeys)
    * Document step-by-step user actions
    * Include wireframes or ASCII sketches

    **Next Steps**: `tech-plan` or `ticket-breakdown`
  </Step>

  <Step title="tech-plan">
    **Purpose**: Create technical implementation plan

    * Define architecture and technical approach
    * Identify files and components to modify
    * Document technical decisions and rationale
    * Reference existing code patterns

    **Next Steps**: `ticket-breakdown`
  </Step>

  <Step title="ticket-breakdown">
    **Purpose**: Break down work into actionable tickets

    * Create independently implementable tickets
    * Link tickets to relevant specs
    * Define acceptance criteria
    * Prioritize and sequence work

    **Next Steps**: Implementation via Phases, Plan, or Agent handoff
  </Step>
</Steps>

### Workflow Philosophy

The Traycer Agile Workflow emphasizes:

* **Collaboration First**: Discuss and align before drafting artifacts
* **Questions as Investments**: Clarification prevents costly mistakes
* **Shared Understanding**: Multiple rounds of questions is normal
* **Readable Artifacts**: Optimize for human parsability


> ## Documentation Index
> Fetch the complete documentation index at: https://docs.traycer.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# YOLO Mode

> YOLO Mode (You Only Look Once) automates your entire workflow. From intelligent orchestration that adapts at runtime to fixed-configuration automation, let Traycer handle planning, coding, and verification without manual intervention.

YOLO Mode brings automated execution to Traycer, minimizing manual intervention and letting you focus on higher-level decisions. Traycer offers two flavors of YOLO:

* **Smart YOLO for Epic Mode**: Intelligent orchestrator that learns from implementation and adapts at runtime—updating specs and tickets, steering plans, and adjusting configurations as it executes Epics end-to-end
* **YOLO for Phases Mode**: Powerful automation using fixed configurations you define upfront for phase-by-phase workflows

Both leverage the same underlying configuration options, but Smart YOLO adaptively evolves your Epic (specs, tickets, plans, and settings) while regular YOLO executes with your predefined configurations.

## Smart YOLO for Epic Mode

Smart YOLO brings intelligent orchestration to [Epic Mode](/tasks/epic), automatically executing entire Epics end-to-end while adapting specs, tickets, and plans based on implementation discoveries. It can run multiple executions in parallel when safe to do so, dramatically reducing overall execution time. Unlike fixed automation, Smart YOLO learns from each execution and steers the Epic dynamically with minimal human intervention.

<Frame>
  <video className="w-full aspect-video rounded-xl" controls autoplay loop muted>
    <source src="https://assets.traycer.ai/smart_yolo_mode.mp4" type="video/mp4" />

    Unable to load video.
  </video>
</Frame>

<Info>
  **Agent Selection**: Smart YOLO can only select from [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />) that are configured in your workspace settings. The orchestrator dynamically chooses the most appropriate agent for each phase based on the task context.
</Info>

### What Smart YOLO does

1. **Evolves your Epic dynamically** - updates specs and tickets at runtime based on implementation discoveries, refining requirements and acceptance criteria as the codebase reveals constraints or opportunities
2. **Steers execution strategy** - analyzes implementation progress and adaptively adjusts plans, breaking down complex tickets or merging related work items as needed
3. **Runs executions in parallel** - intelligently determines which specs and tickets can be executed concurrently without conflicts, significantly reducing overall execution time for independent work items
4. **Creates Executions** - each handoff to a coding agent is tracked as an [Execution](/tasks/epic#executions) in your Epic, providing full visibility into plans, verification results, commits, and status
5. **Makes smart handoffs** - determines optimal execution strategy for each task based on dependencies and implementation context
6. **Adapts all execution settings** - adjusts plans, agents, templates, verification, timeouts, and commits based on requirements and implementation context
7. **Runs verification loops** - validates changes match intent after each execution
8. **Coordinates iterative refinement** - if verification finds issues or implementation reveals scope changes, orchestrates fixes, plan adjustments, and re-verification
9. **Maintains context** - preserves Epic context throughout the entire execution chain, using learnings from one execution to inform subsequent ones

### Triggering Smart YOLO

To start automated execution in Epic Mode:

* Use the `/execute` command in the Epic chat, or
* Simply tell Traycer to execute your tickets or specs (e.g., "Execute these tickets")

Smart YOLO will take over from there, coordinating the entire execution process.

### When to use Smart YOLO

Smart YOLO is ideal when:

* You have specs and tickets ready for implementation (even if they need refinement during execution)
* You want to execute multiple tickets with minimal manual coordination
* You expect implementation to reveal scope changes or technical constraints
* You want an orchestrator that adapts to discoveries rather than blindly following a fixed plan
* You trust your coding agents to handle implementation details while Smart YOLO steers overall strategy
* You want automatic verification after each execution
* You want full execution tracking with plans, verification results, and commits

You describe the Epic and its requirements. Smart YOLO coordinates the rest - adapting specs, steering plans, and managing execution through verification. All handoffs are tracked as [Executions](/tasks/epic#executions) in your Epic view for complete visibility.

<Tip>
  **Auto-commit Configuration**: You can configure Smart YOLO to automatically commit changes after successful execution, creating a clean commit history as your Epic progresses.
</Tip>

### Smart YOLO FAQ

<AccordionGroup>
  <Accordion title="Can Smart YOLO execute multiple specs/tickets in parallel?">
    Yes! Smart YOLO intelligently parallelizes execution to maximize speed while ensuring correctness. It analyzes your Epic to determine:

    * Which specs or tickets are independent and can run concurrently
    * Which work items have dependencies that require sequential execution
    * How to batch related changes to avoid conflicts
  </Accordion>

  <Accordion title="Can I override Smart YOLO's configuration decisions?">
    Smart YOLO operates autonomously with the `/execute` command. If you need more control over specific configurations, you can use manual handoff options from the Epic view where you can specify exact settings for each execution.
  </Accordion>

  <Accordion title="What happens if an execution fails?">
    Smart YOLO monitors execution results and coordinates fixes:

    * If verification finds issues, it can automatically hand off fixes to agents
    * If an execution fails, Smart YOLO pauses to allow you to address the issue
    * You can resume Smart YOLO after resolving failures
    * All execution results are tracked in the [Executions view](/tasks/epic#executions) within your Epic
  </Accordion>

  <Accordion title="Does Smart YOLO work with all agents?">
    No. Smart YOLO only works with [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />) that are configured in your workspace settings. The orchestrator dynamically selects from these configured agents based on the task requirements.
  </Accordion>

  <Accordion title="Can Smart YOLO modify my specs and tickets during execution?">
    Yes! This is a key differentiator of Smart YOLO. Unlike fixed automation, Smart YOLO can:

    * Update specs and tickets based on implementation discoveries
    * Refine acceptance criteria when the codebase reveals constraints
    * Add or modify requirements as execution uncovers scope changes
    * Split complex tickets or merge related ones based on actual implementation needs

    All updates are tracked in your Epic, so you have full visibility into how specs and tickets evolved during execution. This adaptive approach produces better results than rigidly following initial specs that may not account for implementation realities.
  </Accordion>
</AccordionGroup>

For more details on Epic Mode and execution tracking, see the [Epic Mode documentation](/tasks/epic).

## YOLO for Phases Mode

YOLO Mode for [Phases](/tasks/phases) automates the entire phase workflow using **fixed configurations** you define upfront. Once configured, it executes phases consistently without changing settings mid-execution.

### How it works

YOLO Mode works with two types of workflows:

#### Plan Workflow

For implementation tasks, YOLO Mode automatically executes these steps for each phase:

1. **Planning** - Traycer generates detailed plans or skips directly to coding (based on your config)
2. **Coding** - Automatically hands off to your selected coding agent with optional custom templates
3. **Verification** - Validates implementation and optionally hands off selected comment categories back to your agent
4. **Next phase** - Continues to the next phase automatically until all phases are complete

#### Review Workflow

For code review tasks, YOLO Mode automatically executes these steps for each phase:

1. **Review** - Traycer analyzes code and generates review comments
2. **Coding** - Automatically hands off review comments to your selected coding agent for fixes
3. **Next phase** - Continues to the next phase automatically until all phases are complete

The entire cycle runs without manual intervention using your predefined configuration.

### Activating YOLO Mode

<Steps>
  <Step title="1. Create a Phases task">
    Start by creating a task using [Phases Mode](/tasks/phases). YOLO Mode works with the phase structure you've already defined.
  </Step>

  <Step title="2. Click YOLO Mode button">
    Once your phases are displayed in the Kanban board view, click the **YOLO Mode** button to activate automation.
  </Step>

  <Step title="3. Select phase range">
    Use the side slider to select which phases to automate. You can choose from the current phase to any future phase, allowing you to automate a subset of your phases or all remaining phases.
  </Step>

  <Step title="4. Configure automation settings">
    Set up your automation preferences (see [Configuration Options](#configuration-options) below). These settings will be used consistently across all selected phases.
  </Step>

  <Step title="5. Start automation">
    Confirm your configuration and let Traycer run the entire workflow automatically across the selected phase range.
  </Step>
</Steps>

## Configuration options

Both YOLO for Phases and Smart YOLO use the same underlying configuration options. **For regular YOLO**, you set these configurations upfront and they remain fixed throughout execution.

**For Smart YOLO**, the orchestrator goes beyond just configuration—it **evolves your Epic** based on implementation learnings:

**Epic evolution:**

* Updates specs and tickets based on implementation discoveries
* Refines acceptance criteria and requirements
* Steers plans by splitting, merging, or reordering work items
* Propagates learnings from one execution to inform subsequent ones

**Dynamic configuration adjustments:**

* Skip or generate plans
* Select agents (plan, verification, review) from [YOLO-compatible agents](/integrations/agents) configured in your workspace
* Choose templates (plan, verification, review)
* Adjust execution timeouts
* Configure verification severity levels
* Select review categories
* Enable/disable verification
* Enable/disable auto-commits
* Configure custom commit scripts

The orchestrator analyzes each task, learns from implementation progress, and optimizes both Epic content and execution settings for the specific context.

### User query handoff

<Frame>
  <img src="https://mintcdn.com/traycerai/cSD-XeX_zLRyoOFM/images/yolo-user-query-handoff.png?fit=max&auto=format&n=cSD-XeX_zLRyoOFM&q=85&s=e3ad8b138c8ce8820a0c23452597ee4e" alt="User query handoff configuration" className="rounded-lg" width="751" height="870" data-path="images/yolo-user-query-handoff.png" />
</Frame>

Skip detailed planning and send the phase query directly to your coding agent.

**Options:**

* **Skip plan generation**: Check this to bypass detailed plan generation for this phase
* **Execution Agent for user query handoff**: Choose from [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />)
* **Template for user query**: Optionally apply a custom [user query template](/integrations/templates) to wrap the query with additional instructions

**When to use:**

* Simple, straightforward implementation tasks.
* When the phase query is already detailed enough for direct coding.
* To speed up execution for phases with clear requirements.

### Plan handoff

<Frame>
  <img src="https://mintcdn.com/traycerai/cSD-XeX_zLRyoOFM/images/yolo-plan-handoff.png?fit=max&auto=format&n=cSD-XeX_zLRyoOFM&q=85&s=ef53d5e3b1f197cc82bbe74298075b66" alt="Plan handoff configuration" className="rounded-lg" width="755" height="870" data-path="images/yolo-plan-handoff.png" />
</Frame>

Generate a detailed plan and automatically hand it off to your coding agent. This is the default mode when "Skip plan generation" is unchecked.

**Options:**

* **Execution Agent**: Choose from [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />)
* **Template for plan generation**: Optionally apply a custom [plan template](/integrations/templates) to include project-specific instructions, testing requirements, or coding standards

**When to use:**

* Complex implementations requiring structured guidance.
* When you want agents to follow specific architecture patterns.
* For tasks where detailed file-level plans improve code quality.

### Verification handoff

<Frame>
  <img src="https://mintcdn.com/traycerai/cSD-XeX_zLRyoOFM/images/yolo-verification-handoff.png?fit=max&auto=format&n=cSD-XeX_zLRyoOFM&q=85&s=92561abf8796f61be5286f0f3badec67" alt="Verification handoff configuration" width="756" height="870" data-path="images/yolo-verification-handoff.png" />
</Frame>

After Traycer verifies the agent's implementation, automatically send selected comment categories back to the agent for fixes.

**Options:**

* **Skip verification**: Check this to bypass verification for this phase
* **Execution Agent**: Choose from [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />)
* **Template for verification**: Optionally apply a custom [verification template](/integrations/templates) to provide fix instructions
* **Severity levels to verify**: Choose which severity levels to hand off (multiple selections allowed):
  * **Critical**: Blocks core functionality or plan requirements
  * **Major**: Significant issues affecting behavior or UX
  * **Minor**: Small polish items that don't block functionality

**When to use:**

* To automatically iterate on critical and major issues.
* When you want agents to polish implementations before moving to the next phase.
* To maintain quality standards across all phases.

<Tip>
  You can select multiple severity levels to balance quality and speed. For example, selecting only **Critical** and **Major** ensures agents address serious issues while skipping minor polish items.
</Tip>

### Review handoff

<Frame>
  <img src="https://mintcdn.com/traycerai/cSD-XeX_zLRyoOFM/images/yolo-review-handoff.png?fit=max&auto=format&n=cSD-XeX_zLRyoOFM&q=85&s=b6cc75eba62e91893c9008242dbfdd6e" alt="Review handoff configuration" width="773" height="870" data-path="images/yolo-review-handoff.png" />
</Frame>

For Review workflow tasks, automatically hand off review comments to your coding agent for fixes.

**Options:**

* **Execution Agent**: Choose from [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />)
* **Template for review**: Optionally apply a custom [review template](/integrations/templates) to provide fix instructions
* **Review categories**: Choose which review categories to hand off (multiple selections allowed)

**When to use:**

* When working with Review workflow tasks in YOLO Mode
* To automatically address code review feedback
* To maintain code quality standards through automated review cycles

<Tip>
  Review handoff is only available for tasks created with the [Review workflow](/tasks/review).
</Tip>

## Managing agents and templates

### Agents

YOLO Mode (both regular and Smart YOLO) only works with [YOLO-compatible agents](/integrations/agents) that support automated execution. These agents are marked with <Icon icon="bolt" size={16} /> in the agents documentation.

Available agents must be configured in your workspace settings. If your preferred agent isn't available in the dropdown, see [how to add additional agents](/integrations/agents#adding-additional-agents).

For more control over CLI-based agents, you can create [Custom CLI Agents](/integrations/custom-cli-agents) with custom arguments and permissions.

### Templates

To customize handoff prompts with your own instructions, see [Templates documentation](/integrations/templates) for creating user query, plan, and verification templates.

## Preventing interruptions

### Artifact slots and rate limits

YOLO Mode runs continuously across multiple phases, consuming artifact slots as it progresses. If you run out of slots during execution, YOLO Mode will pause until slots are available again.

<Info>
  To avoid interruptions, consider enabling automatic instant refills in your VS Code settings. Learn more about artifact slots, instant refills, and automatic pay-as-you-go in the [Pricing & Usage Limits](/account/pricing#instant-refill) documentation.
</Info>

### Keep your screen active

**Important:** YOLO Mode requires an active connection to your IDE. If your computer goes to sleep or the screen times out, the automation will stop.

**Recommendations:**

* Disable screen timeout/sleep mode during YOLO Mode execution.
* Keep your IDE window active and visible.
* Consider running long YOLO Mode sessions when you can monitor progress.

## FAQ

<AccordionGroup>
  <Accordion title="What happens if YOLO Mode hits a rate limit?">
    YOLO Mode will pause when you run out of artifact slots. You can either wait for slots to recharge based on your plan's recharge rate, or use instant refills (\$0.50 per slot). To avoid interruptions, consider enabling [automatic instant refills](/account/pricing#instant-refill) in your VS Code settings, which will automatically refill slots without prompting. Once slots are available, you'll need to manually resume YOLO Mode from where it stopped.
  </Accordion>

  <Accordion title="Can I change configuration while YOLO Mode is running?">
    Yes, but only for phases that haven't started yet. You can modify configuration for upcoming phases while YOLO Mode is running. However, you cannot change settings for the currently executing phase or phases that have already completed. To adjust settings for future phases, update your configuration and the changes will apply when those phases begin.
  </Accordion>

  <Accordion title="Can I use different agents for different phases?">
    Yes! Each phase can be configured with its own coding agent selection. This allows you to use specialized agents for specific types of work. For example, you might use one agent for frontend phases and another for backend phases.
  </Accordion>

  <Accordion title="What if my computer goes to sleep during execution?">
    YOLO Mode will stop if your computer sleeps or the screen times out. Make sure to disable sleep/timeout settings before starting long automation runs. You can resume from the last completed step once your computer wakes up.
  </Accordion>

  <Accordion title="Does YOLO Mode work with all coding agents?">
    No. YOLO Mode only works with [YOLO-compatible agents](/integrations/agents) (marked with <Icon icon="bolt" size={16} />) that support automated execution. Other agents require manual interaction and are not compatible with YOLO Mode's automated workflow.
  </Accordion>

  <Accordion title="How do templates work with YOLO Mode?">
    Templates wrap Traycer's generated content with your custom instructions. You can select different templates for user query handoff, plan handoff, and verification handoff. This allows you to maintain consistent project standards across automated executions. Learn more in the [Templates documentation](/integrations/templates).
  </Accordion>

  <Accordion title="What's the difference between user query handoff and plan handoff?">
    **User query handoff**: Skips detailed planning and sends the phase query directly to the agent. Faster, but provides less structured guidance.

    **Plan handoff**: Generates a detailed implementation plan first, then hands it to the agent. Takes more time but provides better structure and reduces agent drift.

    Choose based on task complexity and how much guidance your agent needs.
  </Accordion>

  <Accordion title="Can I use Custom CLI Agents with YOLO Mode?">
    Yes! Custom CLI Agents that use YOLO-compatible CLI tools (like Claude Code CLI, Codex CLI, or Gemini CLI) work seamlessly with YOLO Mode. You can create custom templates with special flags like `--dangerous` or custom paths, and they'll appear in the agent selection dropdown. Learn more in the [Custom CLI Agents documentation](/integrations/custom-cli-agents).
  </Accordion>
</AccordionGroup>

## Related documentation

<Columns cols={3}>
  <Card title="Epic Mode" href="/tasks/epic">
    Learn about Epic Mode and how Smart YOLO automates Epic execution.
  </Card>

  <Card title="Phases Mode" href="/tasks/phases">
    Learn about the underlying Phases workflow that YOLO Mode automates.
  </Card>

  <Card title="Plan Workflow" href="/tasks/plan">
    Understand the Plan workflow for implementation tasks.
  </Card>

  <Card title="Review Workflow" href="/tasks/review">
    Understand the Review workflow for code review tasks.
  </Card>

  <Card title="Verification" href="/tasks/verification">
    Learn how Traycer validates agent implementations and categorizes issues.
  </Card>

  <Card title="Supported Agents" href="/integrations/agents">
    Browse the full list of coding agents and add additional agents.
  </Card>

  <Card title="Templates" href="/integrations/templates">
    Create custom Handlebars templates to enhance automated handoffs.
  </Card>
</Columns>

> ## Documentation Index
> Fetch the complete documentation index at: https://docs.traycer.ai/llms.txt
> Use this file to discover all available pages before exploring further.

# Templates

> Customize plan prompts with Handlebars templates for your coding agents

<Frame>
  <iframe className="w-full aspect-video rounded-xl" src="https://www.youtube.com/embed/UNZ_oEVV18A?rel=0" title="Templates" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen />
</Frame>

## What are templates?

Templates are a formatting mechanism that allows you to wrap Traycer's generated content (like plans or verification comments) with your own custom instructions. When you hand off to a coding agent, templates let you include additional context before and after the core content.

For example, when Traycer generates a plan, you can use templates to:

* Add project-specific setup instructions before the plan.
* Include testing requirements after the plan.
* Specify coding standards and conventions.
* Integrate with MCPs or other tools.

### Example template structure

In the following example, `{{planMarkdown}}` is where Traycer inserts the generated plan content, surrounded by your custom instructions. The frontmatter defines the template's display name (`displayName`) and type (`applicableFor`).

```handlebars MyTemplate.md wrap highlight={1-4,8} icon="markdown" theme={null}
---
displayName: Auto-Test Plan Template
applicableFor: plan
---

Follow the below plan verbatim. Trust the files and references. Do not re-verify what's written in the plan.

{{planMarkdown}}

## After Implementation - Always Run Tests
After implementing all the changes above:
1. Run `npm test` to execute the full test suite
2. If any tests fail, fix the issues before considering the implementation complete
3. Ensure all existing tests still pass and new functionality is properly tested
```

## Template types

### Plan Templates (`applicableFor: plan`)

Customize prompts when sending implementation plans to your coding agent.

**Available Handlebars:**

<ParamField query="{{planMarkdown}}" type="placeholder" required>
  Contains the generated implementation plan content.
</ParamField>

### Verification Templates (`applicableFor: verification`)

Customize prompts when sending verification review comments to your agent for fixes.

**Available Handlebars:**

<ParamField query="{{comments}}" type="placeholder" required>
  Contains the verification review comments and feedback.
</ParamField>

### Review Templates (`applicableFor: review`)

Customize prompts when sending review comments to your agent for fixes.

**Available Handlebars:**

<ParamField query="{{reviewComments}}" type="placeholder" required>
  Contains the review comments and feedback.
</ParamField>

### User Query Templates (`applicableFor: user query`)

Customize prompts when bypassing the plan and directly sending the generated user query to your agent for fixes.

**Available Handlebars:**

<ParamField query="{{userQuery}}" type="placeholder" required>
  Contains the generated user query.
</ParamField>

### Generic Templates (`applicableFor: generic`)

Create reusable templates for both plan and verification contexts, or any custom agent interactions.

**Available Handlebars:**

<ParamField query="{{basePrompt}}" type="placeholder" required>
  Contains the base prompt content that can be used in any context.
</ParamField>

<Note>
  More handlebars will be added in the future.
</Note>


