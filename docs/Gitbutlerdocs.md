Guides
Installation and Setup
Installation and setup guide for the GitButler CLI tool.

How to install and setup the GitButler CLI.

Installing the but CLI
Ok, first thing is first, let's get our but CLI installed. Currently there are two ways to do this.

Via the Desktop Client
If you have the desktop client installed, you can go into your global settings and click on the "Install CLI" button in the "general" section.

How to install the GitButler CLI
How to install the GitButler CLI
Curl install
You can install the CLI by running the following command in your terminal:


curl -fsSL https://gitbutler.com/install.sh | sh
Setup
If you go into any existing Git repository and run but setup, it will make some neccesary changes to your setup in order for GitButler to manage your data.

If you run almost any but command in an existing Git repository in an interactive terminal, it will ask you if you want to set it up and then run the command you were trying to run. So basically just run but anywhere to get started.

At any time after this, you can run but teardown to undo the GitButler changes and go back to being a boring old Git project. It will not remove GitButler metadata, so feel free to go back and forth if you need to.

Butler Flow
Discover Butler Flow, a lightweight branch-based development workflow that improves upon GitHub Flow with GitButler's parallel branches.

Butler Flow is a lightweight, branch-based workflow enabled by GitButler's virtual branch functionality.

For most modern software development teams that want to set up a culture of shipping, who push to production every day, who are constantly testing and deploying, one of the most popular and effective development workflows is GitHub Flow.

These are wise words, enterprising men quote 'em. Don't act surprised, you guys, cuz I wrote 'em.

However, a decade later, and with a new and more powerful branching toolset, we're able to amend that basic workflow to be simpler, faster, more flexible, and less error prone. Let's take a quick minute to explore what the Butler Flow is for software development teams using GitButler and how this can make all of our lives easier.

Overview
In a nutshell, the basic development cycle is very simple.

All work is based off a "target branch", which is a representation of released production code.
All work immediately exists in a branch.
Work can be shared with teammates for review as early as possible.
All branches that are close to merging can be applied locally for integration testing.
Branches can be reviewed independently and merged after final review.
Integrated branches are automatically removed from developer context.
The Target Branch
In stock, vanilla Git tooling, there is nothing specified as the production branch, no special "trunk". It is only by convention that this is enforced.

In GitButler, branches will not work without the specification of a special "target branch". Everything exists in relation to this special branch, everything that differs from it must be accounted for by being owned by some other branch, until those changes are integrated.

Parallel Branches
Once you choose a target branch, everything that is in your working directory and not ignored by the Git Ignore specification must be owned by a virtual branch. If you don't have one active, GitButler will automatically create one for you.

All subsequent changes to your working directory, either by applying other branches or directly modifying files, must be owned by a virtual branch.

Branches are meant to be small, independent and rapidly integrated. However, longer lived branches can be continuously re-integrated, keeping them clean and mergeable for long periods of time if needed, while still being shareable and reviewable with your team.

Parallel branches can be started and ended entirely independently of each other. Developers can work on longer branches, while starting, reviewing, finishing, merging, and deleting small ones without ever changing branch context.

Collaboration
All your team's work, whether created and managed by GitButler or not, exists on the central server as normal Git branches. These will automatically be pulled down and kept up to date by GitButler and can be converted into parallel branches and applied to your working directory in addition to your branches.

This allows you to integrate work from the rest of your team continuously and early, while still keeping non-dependent changes separated and independently reviewable and mergable. It allows you to review code without needing to entirely switch contexts, and to do so early and often in an executable environment.

Merge conflicts are known almost as soon as they occur and can be communicated about and collaborated on far before an upstream merge.

Maintenance
Parallel branches can remain applied locally until they are merged into your upstream target branch. Once integrated by any fashion (squash merge, rebase, merge), the virtual branch is automatically disposed of, keeping cruft and overhead low.

Features
Branch Management
Rules
Automate change assignment with rules that match file patterns and automatically route changes to the right branch.

Rules are a powerful automation feature in GitButler that automatically assign file changes to specific branches based on conditions you define. Instead of manually dragging changes between lanes, you can set up rules that automatically route changes where they belong.

Overview
When you're working on multiple branches simultaneously, you often know in advance which types of changes should go to which branch. For example, documentation updates might always go to a docs branch, while UI changes go to a feature/ui-redesign branch.

Rules eliminate the manual work of assigning changes by automatically evaluating your uncommitted changes and routing them to the appropriate branch based on filters you define.

How Rules Work
Rules are evaluated whenever files change in your working directory (the fileSytemChange trigger). Each rule consists of:

Filters: Conditions that determine which changes the rule applies to
Action: Assigns matching changes to a specific branch
Multiple rules can exist, and they are evaluated in order. Within a single rule, multiple filters are combined with AND logic - all conditions must match for the rule to apply.

Creating a Rule
To create a rule:

Open the Rules drawer at the bottom of the GitButler interface
Click the + button to add a new rule
Select the target branch where matching changes should be assigned:
Specify a branch by name
Leftmost lane: The leftmost branch in your workspace
Rightmost lane: The rightmost branch in your workspace
(Optional) Add filters to specify which changes should match
Click Save rule
If you don't add any filters, the rule will match all changes.

Creating a rule that assigns changes to the leftmost lane


Filter Types
Rules support filters that match file paths or changed line content.

Path Matches Regex
Matches file paths using a regular expression pattern.

Example use cases:

Match all TypeScript files: .*\.ts$
Match files in a specific directory: ^src/components/.*
Match documentation files: .*\.(md|mdx)$
Content Matches Regex
Matches the content of changed lines using a regular expression pattern. This filter only looks at added lines (lines that start with + in the diff).

Example use cases:

Match changes containing TODOs: TODO
Match changes with specific function calls: console\.log
Match changes with certain patterns: @deprecated
Creating a rule that assigns changes containing "fix" to a specific branch


Managing Rules
Editing Rules
To edit an existing rule:

Double-click the rule or click the ellipsis menu (...) and select "Edit rule"
Modify the branch assignment or filters
Click Save rule
Deleting Rules
To delete a rule:

Click the ellipsis menu (...) on the rule
Select "Delete rule"
Confirm the deletion
Understanding Rule Evaluation
Order Matters
Rules are evaluated in the order they appear in the Rules drawer (most recent first). The first matching rule determines where a change is assigned.

AND Logic Within Rules
When a rule has multiple filters, all filters must match for the rule to apply. For example, a rule with both "Path Matches Regex: .*\.ts$" and "Content Matches Regex: TODO" will only match TypeScript files that contain the text "TODO" in their changes.

OR Logic Across Rules
If you want to match changes that meet any of several conditions (OR logic), create separate rules for each condition.

Interaction with Hunk Dependencies
Rules respect hunk dependencies (locks). If a change depends on a commit in a specific branch, it cannot be automatically reassigned by rules, even if it matches a rule's filters.

Best Practices
Start simple: Begin with one or two basic path-matching rules before adding complex filters
Order your rules: Place more specific rules before general catch-all rules
Test your regex: Make sure your regular expressions match what you intend - it's easy to be too broad or too narrow
Use catch-all rules carefully: A rule with no filters will match everything, which can interfere with other rules
Consider your workflow: Rules work best when you have predictable patterns in how your work is organized
Leverage leftmost/rightmost: Using position-based targeting lets you reorganize lanes without updating rules
Limitations
Rules can only assign changes to branches that exist in your workspace (applied branches)
Rules currently only support the assign action for filesystem changes
Related Features
Parallel Branches: Understanding the branch system that rules work with
Branch Lanes: How lanes are organized and how rules interact with lane positioning

Features
Branch Management
Parallel Branches
Understanding GitButler's parallel branches that allow simultaneous work on multiple branches in a single working directory.

Parllel branches are a powerful feature of GitButler that allow you to work on multiple branches at the same time, committing to them independently and simultaneously. This is a key part of the GitButler experience, allowing you to manage your work in a flexible and efficient way that is not possible with traditional Git tooling.

Overview
With normal Git branching, you can only work on one branch at a time. There is one HEAD reference and one index.

With parallel branches, you can have multiple branches applied to your working directory at the same time. Each branch is represented as a vertical lane, and you can drag changes between these lanes to commit them independently.

Each lane also has its own staging area, so you can stage changes for each branch before deciding to commit them.

A project with three simultaneous branches, plus some staged changes in each one and some unstaged (unassigned) changes.
A project with three simultaneous branches, plus some staged changes in each one and some unstaged (unassigned) changes.
How it works
Let's say that you make changes to two different files and git status would list two modified files. In GitButler, you can "assign" the change in each file to a different "virtual" branch, then when you commit, it will create a commit that only contains the changes in that file for that branch.

One of the nice things with this approach is that since you're starting from changes in a single working directory, you can be sure that all branches that you create from it will merge cleanly, as you're essentially starting from the merge product and extracting branches of work from it.

Features
Branch Management
Branches Page
Manage parallel branches, remote branches, and workspace targets in GitButler's branch management interface.

All of your branches - remote, local, and virtual / applied or not - are managed in the Branch Tab. This is where you can see all of your branches, apply them to your workspace, and manage your parallel branches.

You can access the Branches tab by clicking on the "Branches" icon in the sidebar.

The interface looks something like this:

The branch tab
The branch tab
Branch List
The first pane on the left shows you the parallel branches and stacks that you have as well as the other branches that you have available (legacy git branches, remote branches and PRs).

All of these branches can be converted into parallel branches by clicking them and then clicking the "Apply to workspace" button on the top of the branch view (middle pane).

Local branches can also be fully deleted here.

Current Workspace Target
The "Current workspace target" is the view of the target branch that you've set. It will show you essentially a git log of origin/master or whatever you set as your target branch, and it will show you if there are any commits upstream that you have not integrated locally yet. We will automatically check for new upstream changes every few minutes, but you can also click the update button to check immediately.

Features
Branch Management
Rules
Automate change assignment with rules that match file patterns and automatically route changes to the right branch.

Rules are a powerful automation feature in GitButler that automatically assign file changes to specific branches based on conditions you define. Instead of manually dragging changes between lanes, you can set up rules that automatically route changes where they belong.

Overview
When you're working on multiple branches simultaneously, you often know in advance which types of changes should go to which branch. For example, documentation updates might always go to a docs branch, while UI changes go to a feature/ui-redesign branch.

Rules eliminate the manual work of assigning changes by automatically evaluating your uncommitted changes and routing them to the appropriate branch based on filters you define.

How Rules Work
Rules are evaluated whenever files change in your working directory (the fileSytemChange trigger). Each rule consists of:

Filters: Conditions that determine which changes the rule applies to
Action: Assigns matching changes to a specific branch
Multiple rules can exist, and they are evaluated in order. Within a single rule, multiple filters are combined with AND logic - all conditions must match for the rule to apply.

Creating a Rule
To create a rule:

Open the Rules drawer at the bottom of the GitButler interface
Click the + button to add a new rule
Select the target branch where matching changes should be assigned:
Specify a branch by name
Leftmost lane: The leftmost branch in your workspace
Rightmost lane: The rightmost branch in your workspace
(Optional) Add filters to specify which changes should match
Click Save rule
If you don't add any filters, the rule will match all changes.

Creating a rule that assigns changes to the leftmost lane


Filter Types
Rules support filters that match file paths or changed line content.

Path Matches Regex
Matches file paths using a regular expression pattern.

Example use cases:

Match all TypeScript files: .*\.ts$
Match files in a specific directory: ^src/components/.*
Match documentation files: .*\.(md|mdx)$
Content Matches Regex
Matches the content of changed lines using a regular expression pattern. This filter only looks at added lines (lines that start with + in the diff).

Example use cases:

Match changes containing TODOs: TODO
Match changes with specific function calls: console\.log
Match changes with certain patterns: @deprecated
Creating a rule that assigns changes containing "fix" to a specific branch


Managing Rules
Editing Rules
To edit an existing rule:

Double-click the rule or click the ellipsis menu (...) and select "Edit rule"
Modify the branch assignment or filters
Click Save rule
Deleting Rules
To delete a rule:

Click the ellipsis menu (...) on the rule
Select "Delete rule"
Confirm the deletion
Understanding Rule Evaluation
Order Matters
Rules are evaluated in the order they appear in the Rules drawer (most recent first). The first matching rule determines where a change is assigned.

AND Logic Within Rules
When a rule has multiple filters, all filters must match for the rule to apply. For example, a rule with both "Path Matches Regex: .*\.ts$" and "Content Matches Regex: TODO" will only match TypeScript files that contain the text "TODO" in their changes.

OR Logic Across Rules
If you want to match changes that meet any of several conditions (OR logic), create separate rules for each condition.

Interaction with Hunk Dependencies
Rules respect hunk dependencies (locks). If a change depends on a commit in a specific branch, it cannot be automatically reassigned by rules, even if it matches a rule's filters.

Best Practices
Start simple: Begin with one or two basic path-matching rules before adding complex filters
Order your rules: Place more specific rules before general catch-all rules
Test your regex: Make sure your regular expressions match what you intend - it's easy to be too broad or too narrow
Use catch-all rules carefully: A rule with no filters will match everything, which can interfere with other rules
Consider your workflow: Rules work best when you have predictable patterns in how your work is organized
Leverage leftmost/rightmost: Using position-based targeting lets you reorganize lanes without updating rules
Limitations
Rules can only assign changes to branches that exist in your workspace (applied branches)
Rules currently only support the assign action for filesystem changes
Related Features

Overview
Use GitButler to keep coding-agent changes organized into branches and commits you can inspect and review.

Use GitButler with coding agents when you want messy local changes to become reviewable branches and commits without moving every task into a separate worktree.

GitButler keeps one working directory while organizing changes into separate branches and commits. Agents can do that through the but CLI, and you can inspect the same state in the Desktop Client. For when to use GitButler instead of separate worktrees, see Parallel agents.

You decide how far the agent goes. You can tell it to stop after local commits, or you can allow it to push and open pull requests. GitButler gives the agent version-control commands; your instructions decide when it stops.

What agents can do with GitButler
Parallel branches in one workspace. An agent can create separate branches for independent work while staying in the same working directory. That gives you separate review branches without creating a new worktree, directory, and setup for each task.
Selected files and hunks. The agent can assign selected files or hunks to a branch, then commit that branch. You can inspect the split before the work is pushed.
Stacked branches for dependent work. If one change depends on another, the agent can put the dependent branch on top of the branch it needs.
History edits without an interactive rebase. The agent can move a change from one commit to another, reorder commits, reword commits, absorb fixes, or split a commit with but commands.
Review and recovery before publishing. You can inspect the same branches and commits in the CLI or Desktop Client, then use but oplog to restore an earlier local GitButler state if the branch layout needs to be rolled back.
History edits as direct commands
History edits are where agent workflows often get brittle. Moving one file's changes from one commit to another with Git can involve patch restore/reset steps, fixup commits, autosquash, or an interactive rebase with one or more edit stops.

With GitButler, that kind of edit becomes a direct operation: move this file's changes into that commit. The operation can still rewrite commits and can still conflict. The difference is control flow: the agent gets targeted tools for the edit instead of driving a multi-step rebase session.

For more detail, see the but rub command reference and the CLI guide to moving changes between commits.

GitButler does not currently focus on a direct "push to main" workflow. Today, it is best used to get from agent changes to clean local branches, commits, and PRs or MRs quickly.

Parallel agents
Run multiple coding agents in one GitButler workspace without creating separate worktrees.

Parallel agents do not require separate worktrees or pre-created branches. Once agents use GitButler for version-control writes, you can start another coding session in the same repository and prompt it like any other task:


Work on checkout validation.
When either session is ready, ask it to commit:


Commit your changes.
The agent uses GitButler to commit the changes for its task to its GitButler branch. The branch routing is the agent's job; your prompt can stay small.

If two sessions touch the same file or generated output, have the agents call out the overlap before committing.

You can also ask an agent to split independent work out of the current session. For example, if a feature session also finds a small bug fix, the agent can move the relevant changes or commits to a new branch and prepare a separate PR instead of stacking the fix on the feature.

For more background on the branch model, see Parallel branches.

The baseline Version control instructions from Getting started are useful if you want to steer commit behavior, but they are not a separate parallel-agent setup step.

How this differs from worktrees
Git has one checked-out branch per worktree. If you want two agents to work on two branches at the same time, the usual Git answer is multiple worktrees.

GitButler gives you a different option: multiple active branches in one worktree, with each agent's commits organized onto the branch for its session.

Multiple worktrees	GitButler parallel branches
Workspace	One directory per agent	One shared working directory
Branches	One checked-out branch per worktree	Multiple active branches in one worktree
Isolation	Separate checkout	Shared filesystem and runtime state
Setup cost	Usually more directories, dependency installs, build outputs, and dev servers	Reuse one install and dev server when tasks can share runtime state
Version-control shape	Branches stay separate because work happens in separate directories	GitButler can commit the right subset of changes to each branch
Best fit	Competing attempts, incompatible checkout states, isolated runtimes	Unrelated features or fixes that can share one workspace
Use multiple worktrees when agents need incompatible checkout states, separate runtime state, or competing attempts at the same task. Use GitButler parallel branches when the tasks are independent enough to share one workspace and you want less local overhead.

Handle dependencies explicitly
Parallel agents work best when sessions start independent. If one session starts depending on another, make that relationship explicit by stacking the branches:


The notification settings work now depends on checkout validation. Stack your
branch on top of the checkout validation branch.
If an unrelated fix shows up inside a feature session, tell the agent to extract it instead:


The cache invalidation fix is independent. Move it to a separate GitButler
branch and prepare a separate PR for it.
If the feature depends on the fix, put the fix on the lower branch and stack the feature above it instead. For stacked PR policy, see Create stacked pull requests.

Know what is shared
Parallel GitButler branches are not runtime isolation. The agents share one filesystem, dependency install, generated files, and app state. That can surface overlap and broken builds earlier, but it can also hide accidental dependencies.

Before shipping a branch independently, check whether it depends on another active branch. If two agents start editing the same files or generated output, decide whether to keep the work parallel, stack one branch on the other, or use separate worktrees.

For more request examples, see Useful requests.

Tuning agent behavior
Add optional instruction snippets that steer how coding agents amend, checkpoint, stack, publish, and recover work with GitButler.

Add these optional bullets under the same ## Version control section as the baseline from Getting started. Use this page as a menu: copy only the policies you want for the repository and agent you are using.

Amend local fixes into the right commits
Use this when you want the agent to fold follow-up fixes into unpublished local commits when the new change clearly belongs with that commit's intent. With GitButler, the agent can move the relevant change into the commit where it belongs.


- For small cleanup or follow-up fixes, amend an unpublished local commit when
  the change clearly belongs with that commit's intent.
- Do not create tiny fixup commits unless I ask.
- Use GitButler to move the relevant changes into the commit where they belong.
- Ask before rewriting pushed, reviewed, shared, or ambiguous history.
You do not need to tell the agent which command to use. The GitButler skill gives it the relevant operations. For background, see but absorb and but amend.

Commit checkpoints after each completed turn
Use this when you want local savepoints while the agent works. The checkpoints do not need to be the final review history. Before review, you can ask the agent to tidy unpublished local history.


- Commit after a working checkpoint, when the requested change is complete and
  relevant checks have passed or been reported.
- Treat checkpoint commits as local savepoints, not final review history.
- When I ask you to tidy the history, use GitButler to squash commits, reword
  commits, and move changes between commits where appropriate.
- Only tidy unpublished local history unless I explicitly authorize changing
  pushed or shared history.
Create stacked pull requests
Use this when you want dependent work reviewed as stacked pull requests. This is useful when one agent session depends on another session's branch, or when an agent is working on a branch that sits at the bottom of a stack.


- If this session depends on another in-flight branch, stack its branch on top
  of that dependency instead of mixing the changes.
- If this session is working in a stack, put commits on the branch where they
  belong.
- Ask before moving commits onto lower, pushed, reviewed, or shared branches.
- Use `but move` for branch stacking and restacking. Do not recreate branches
  to simulate stacking.
- For stacked branches, create pull requests with `but pr`, not `gh`, so
  GitButler keeps the right PR base branches and stack metadata.
For background, see Stacked branches, but move, and but pr.

Customize branch names
Use this when your team has a naming convention for branches the agent creates. This is only an example; replace the prefix and shape with your convention.


- When creating a GitButler branch for an agent session, use
  `feature/<ticket-id>-<short-description>` when a ticket ID is available.
Customize commit messages
Use this when your team has a commit-message convention. This is only an example; replace it with your preferred style.


- Use Conventional Commits, such as `feat: add branch naming policy` or
  `fix: handle empty branch names`.
Publish when you say "ship it"
Use this when you want a short phrase to authorize the agent to finish the version-control work for its session. This commits, pushes, and creates or updates a pull request, so use it only when the agent is allowed to publish.


- When I say "ship it", commit this session's changes on its dedicated
  GitButler branch, creating one if needed.
- Push the branch and open or update its pull request with GitButler.
- Reuse the existing branch or pull request for this session when one already
  exists.
For background, see but push and but pr.

Update from main automatically
Use this when your project moves quickly and you want the agent to keep its workspace current with the target branch, usually main or master. The GitButler command for this is but pull, which fetches the target branch and rebases applied branches onto the new target commit. This is a preference: in some repositories, you may want the agent to ask before updating.

Add the last bullet only if you want the agent to handle update conflicts.


- When GitButler status shows new changes on the target branch, run
  `but pull --check`.
- If the check is clean and the update affects only this session's branches,
  update the workspace with `but pull`.
- If the check reports conflicts or the update would affect another agent's
  branch, ask before updating.
- If I ask you to handle update conflicts, use GitButler's conflict tools. Ask
  before resolving semantic conflicts, dependency updates, generated files, or
  conflicts involving another person's work.
You do not need to tell the agent which command to use. For background, see but pull and but resolve.

Open draft pull requests by default
Use this when the agent is allowed to publish work, but you still want review to start in draft. Creating a draft pull request still publishes the branch.


- When I ask you to open a pull request, create it as a draft with GitButler
  unless I say it is ready for review.
Create a recovery point before large history edits
Use this when you want the agent to be more cautious before reorganizing several commits or branches.


- Before squashing, splitting, moving commits between branches, or reorganizing
  multiple branches, run `but oplog snapshot -m "<reason>"`.
- Use GitButler history-edit commands such as `but move`, `but squash`,
  `but reword`, `but absorb`, and `but amend` instead of raw Git rebases.
- If an operation makes the branch or history layout worse, stop and inspect the
  operation log before attempting another fix.
- Prefer `but undo` or `but oplog restore` over trying to repair a bad state
  with more history edits.
For command details, see but oplog and but undo.

Split unrelated hunks
Use this when agents tend to commit whole files even when one file contains separate changes.


- If one file contains unrelated changes, split them by hunk instead of
  committing the whole file.
- Keep tests with the behavior they verify.
- Split generated output, docs-only edits, or mechanical cleanup into separate
  commits when each commit remains coherent on its own.
- If the split is ambiguous, summarize the options before committing.

Inspect and adjust agent work
Choose a GitButler surface for previewing and adjusting coding-agent branch and commit history.

When your coding agent uses GitButler, it can do the version-control work for you: create branches, commit changes, move work between commits, and reshape local history. The agent usually does this through the GitButler CLI.

You may still want to preview what it created or manually adjust the branch and commit history before anything is pushed or turned into a PR. GitButler gives you three ways to inspect and adjust that state: the TUI, the CLI, and the Desktop Client GUI. Use this page to pick the surface you want. The detailed workflows live in their own docs.

GitButler TUI
Use the TUI when you want a terminal view of the workspace without switching to the Desktop Client. It is useful for checking branch state, looking at what is on a branch or still unassigned, and making small manual adjustments to branch assignment, commit membership, or history shape.


but tui
For the full workflow and keybindings, see the TUI guide.

GitButler TUI showing available keyboard commands over workspace status
GitButler TUI command view.
GitButler CLI
Use the CLI when you want exact output, scriptable commands, or the same view of the repository state that the agent uses.

For command details, start with the CLI overview.

GitButler Desktop Client
Use the Desktop Client when you want a visual overview of branches, commits, assigned changes, unassigned changes, parallel work, and stacked work. It is also the visual surface for moving changes between branches and adjusting commit history.


but gui
For details, see the Desktop Overview, Branch lanes, and Commits.

Operations history
GitButler records version-control operations so you can inspect or undo local history edits. If a branch reorganization does not look right, inspect the operation history before making more changes. See Timeline, but oplog, and but undo.

Commands
Commands Overview
Your fast index to all of the available commands

Command reference
Basics
setup: Set up a Git repository to be managed by GitButler
teardown: Exit GitButler mode and return to normal Git
Inspection
status: Show the project workspace state
diff: Show a diff for the workspace or a CLI ID
show: Show information about a commit or branch
Branching and committing
commit: Commit changes to a stack
stage: Stage changes to a branch
branch: Commands for managing branches
merge: Local branch merging
discard: Discard uncommitted changes
resolve: Resolve conflicts in a commit
unapply: Unapply a branch from the workspace
apply: Apply a branch to the workspace
clean: Remove empty branches from the workspace
pick: Cherry-pick a commit from an unapplied branch
Rules
mark: Create or remove a rule for auto-assigning or auto-committing
unmark: Remove all marks from the workspace
Server interactions
push: Push changes in a branch to remote
pull: Pull upstream changes and update your branches
pr: Create and manage reviews on GitHub, GitLab, and other forges
Editing commits
rub: Combine two entities to amend, squash, move, or unassign work
absorb: Amend changes into the commits where they belong
reword: Edit a commit message or branch name
uncommit: Move commit changes back to unassigned work
amend: Amend a file change into a commit
squash: Squash commits together
move: Move a commit or branch to a different location
Operations log
oplog: View and manage operation history
undo: Undo the last operation
redo: Redo the last undo
Helper commands
gui: Open the GitButler GUI for the current project
tui: Open the interactive terminal UI
update: Manage GitButler CLI and app updates
alias: Manage command aliases
config: View and manage GitButler configuration
skill: Manage AI agent skills for GitButler

Commands
but setup
Sets up a GitButler project from a git repository in the current directory.

This command will:

Add the repository to the global GitButler project registry
Switch to the gitbutler/workspace branch (if not already on it)
Set up a default target branch (the remote's HEAD)
Add a gb-local remote if no push remote exists
If you have an existing Git repository and want to start using GitButler with it, you can run this command to set up the necessary configuration and data structures.

Examples
Initialize a new git repository and set up GitButler:


but setup --init
Usage: but setup [OPTIONS]

Options
--init — Initialize a new git repository with an empty commit if one doesn't exist.
This is useful when running in non-interactive environments (like CI/CD) where you want to ensure a git repository exists before setting up GitButler.

Commands
but teardown
Exit GitButler mode and return to normal Git workflow.

This command:

Creates an oplog snapshot of the current state
Finds the first active branch and checks it out
Alternatively, use --checkout-to <branch> to override this default
Cherry-picks any dangling commits from gitbutler/workspace
Provides instructions on how to return to GitButler mode
This is useful when you want to temporarily or permanently leave GitButler management and work with standard Git commands.

Examples
Exit GitButler mode:


but teardown

but teardown --checkout-to my-feature-branch
Usage: but teardown [OPTIONS]

Options
-c, --checkout-to <LOCAL_BRANCH> — Explicit override for which local branch to checkout to

Commands
but mark
Mark a commit or branch for auto-stage or auto-commit.

Creates or removes a rule for auto-staging or auto-committing changes to the specified target entity.

If you mark a branch, new unstaged changes that GitButler sees when you run any command will be automatically staged to that branch.

If you mark a commit, new uncommitted changes will automatically be amended into the marked commit.

Usage: but mark <TARGET> [OPTIONS]

Arguments
<TARGET> — The target entity that will be marked (required)
Options
-d, --delete — Deletes a mark

Workspace Branch
What is the `gitbutler/workspace` branch and merge commit it points to? Also, why do we need it?

If you run some normal Git commands (like git log) while in GitButler mode, you'll see a few special branches that GitButler maintains behind the scenes. The one that most people get confused by is the gitbutler/workspace commit.

There are a few different reasons that we need it, so let's take a quick look.

If you run a normal git log on a GitButler managed repository, you will see something like this:


commit de56d20e282f7641d48d288b510141996c3c3cfc (HEAD -> gitbutler/workspace)
Author: GitButler <gitbutler@gitbutler.com>
Date:   Wed Sep 9 09:06:03 2020 +0800

    GitButler Workspace Commit

    This is is a merge commit of the parallel branches in your workspace.

    For GitButler to manage multiple parallel branches, we maintain
    this commit automatically so other tooling works properly.

    If you switch to another branch, GitButler will need to be
    reinitialized.

    Here are the branches that are currently applied:

     - update-homepage (refs/gitbutler/update-homepage)
       branch head: a32f33273948837078e5f5a4e1677ab6274a4629

    For more information about what we're doing here, check out our docs:
    https://docs.gitbutler.com/workspace-branch

commit a32f33273948837078e5f5a4e1677ab6274a4629 (update-homepage)
Author: Scott Chacon <schacon@gmail.com>
Date:   Mon Jan 26 07:33:31 2026 +0500

    hero update - new branding

That first commit is a merge commit that we rebuild as you modify branches in GitButler. The reason that it exists is mainly because if you have more than one branch applied in your workspace, when other tools run git status, it will look strange, since Git has no concept of having several branches applied at once.

Status, Diff and Log
To keep Git command output for things that look at the index and HEAD (such as status or diff) somewhat sane, we modify your index to look like the union of all the committed states of all your applied parallel branches. This makes git diff and git status behave more or less like you would expect.

For instance, if you have two files on Branch A and two files on Branch B, then git status will simply list four files as modified.

If you run git log, the first commit should be our custom commit message and the tree of that commit is the union of all the committed work on all your applied parallel branches, as though they were all merged together into one (something stock Git can understand).

Committing, Branching, Checking Out
However, if you try to use something that writes to HEAD, like git commit or git checkout, then you might have some headaches. For this reason, we install custom Git hooks for pre-commit and post-checkout that will protect this from happening.

If you try to commit when in GitButler managed mode, the pre-commit hook should disallow it and tell you how to fix it.


❯ git commit -am 'commit on the workspace branch'

GITBUTLER_ERROR: Cannot commit directly to gitbutler/workspace branch.

GitButler manages commits on this branch. Please use GitButler to commit your changes:
  - Use the GitButler app to create commits
  - Or run 'but commit' from the command line

If you want to exit GitButler mode and use normal git:
  - Run 'but teardown' to switch to a regular branch
  - Or directly checkout another branch: git checkout <branch>

If you no longer have the GitButler CLI installed, you can simply remove this hook and checkout another branch:
  rm ".git/hooks/pre-commit"
If you want to get out of this mode, you can follow any of those instructions. The easiest is running but teardown, but simply switching directly to a normal Git branch will also do the trick.

Debugging
Troubleshooting guide for GitButler technical issues including logs, data files, and platform-specific debugging tips.

If you are having technical issues with the GitButler client, here are a few things you can do to help us help you. Or help yourself.

If you get stuck or need help with anything, hit us up over on Discord, here's GitButler Discord Server Link.


The first things to try is checking out the frontend related logs in the console by opening the developer tools in GitButler via the "View" -> "Developer Tools" menu option. Next, if you launch GitButler from the command line, you can view the backend logs directly in your terminal.

Logs
Often the most helpful thing is to look at the logs. GitButler is a Tauri app, so the logs are in your OS's app log directory. This should be:

macOS
Windows
Linux

~/Library/Logs/com.gitbutler.app/
In this directory, there should be rolling daily logs:

Terminal

❯ cd ~/Library/Logs/com.gitbutler.app
 
❯ tree -L 1
 
├── GitButler.log
├── GitButler.log.2023-09-02
├── GitButler.log.2023-09-03
├── GitButler.log.2023-09-04
├── GitButler.log.2023-09-05
├── GitButler.log.2023-09-06
├── GitButler.log.2023-09-07
├── GitButler.log.2023-09-08
├── GitButler.log.2023-10-10
├── GitButler.log.2024-01-30
└── tokio-console
 
❯ tail GitButler.log.2024-01-30
2024-01-30T13:02:56.319843Z  INFO get_public_key: gitbutler-app/src/keys/commands.rs:20: new
2024-01-30T13:02:56.320000Z  INFO git_get_global_config: gitbutler-app/src/commands.rs:116: new key="gitbutler.utmostDiscretion"
2024-01-30T13:02:56.320117Z  INFO git_get_global_config: gitbutler-app/src/commands.rs:116: new key="gitbutler.signCommits"
2024-01-30T13:02:56.320194Z  INFO get_public_key: gitbutler-app/src/keys/commands.rs:20: close time.busy=317µs time.idle=47.0µs
2024-01-30T13:02:56.320224Z  INFO git_get_global_config: gitbutler-app/src/commands.rs:116: close time.busy=204µs time.idle=25.3µs key="gitbutler.utmostDiscretion"
2024-01-30T13:02:56.320276Z  INFO git_get_global_config: gitbutler-app/src/commands.rs:116: close time.busy=133µs time.idle=35.8µs key="gitbutler.signCommits"
2024-01-30T13:02:56.343467Z  INFO menu_item_set_enabled: gitbutler-app/src/menu.rs:11: new menu_item_id="project/settings" enabled=false
2024-01-30T13:02:56.343524Z  INFO menu_item_set_enabled: gitbutler-app/src/menu.rs:11: close time.busy=35.7µs time.idle=28.8µs menu_item_id="project/settings" enabled=false
Data Files
GitButler also keeps its own data about each of your projects. The virtual branch metadata, your user config stuff, a log of changes in each file, etc. If you want to inspect what GitButler is doing or debug or reset everything, you can go to our data directory.

macOS
Windows
Linux

~/Library/Application Support/com.gitbutler.app/
In this folder there are a bunch of interesting things.

Terminal

❯ cd ~/Library/Application\ Support/com.gitbutler.app
 
❯ tree
.
├── keys
│   ├── ed25519
│   └── ed25519.pub
├── projects.json
└── settings.json
 
4 directories, 4 files
The projects.json file will have a list of your projects metadata:

Terminal

❯ cat projects.json
[
  {
    "id": "71218b1b-ee2e-4e0f-8393-54f467cd665b",
    "title": "gitbutler-blog",
    "description": null,
    "path": "/Users/scottchacon/projects/gitbutler-blog",
    "preferred_key": "generated",
    "ok_with_force_push": true,
    "api": null,
    "gitbutler_data_last_fetch": null,
    "gitbutler_code_push_state": null,
    "project_data_last_fetch": {
      "fetched": {
        "timestamp": {
          "secs_since_epoch": 1706619724,
          "nanos_since_epoch": 202467000
        }
      }
    }
  }
]
The settings.json are some top level preferences you've set.

Terminal

❯ cat settings.json
{
  "appAnalyticsConfirmed": true,
  "appNonAnonMetricsEnabled": true
}
Finally, the keys directory holds the SSH key that we generate for you in case you don't want to go through creating your own. It's only used if you want to use it to sign commits or use it for authentication.

Linux
glibc Errors
The Linux installation is currently being built in a GitHub Action with Ubuntu 24.04. This means support is limited to those installations using the same or newer version of glibc. Unfortunately we cannot build using earlier versions of Ubuntu due to another incompatibility with libwebkit2gtk-4.1 and Tauri at the moment.

If you're using an older distribution, you may be interested in trying our Flatpak package available on Flathub.

Failed to create EGL image from DMABuf
If you start GitButler from the command line and see a bunch of these or similar EGL / DMABuf related messages printed to the console and are only getting a white screen to render, you can try launching GitButler with the following environment variables:

WEBKIT_DISABLE_DMABUF_RENDERER=1
WEBKIT_DISABLE_COMPOSITING_MODE=1
This issue most likely stems from an incompatibility between your version of OpenGL (mesa) and libwebkit2gtk-4.1.

Commands
but skill
Manage AI agent skills for GitButler.

Skills provide enhanced AI capabilities for working with GitButler through Claude Code, Codex, and other AI assistants.

Use but skill install to install the GitButler skill files. By default, it prompts for scope (repository or global home directory) and then format. When run outside a git repository, local scope is unavailable and the default install location is global (home directory). You can still install to a custom location with --path using an absolute or ~ path.

Examples
Install interactively (prompts for scope and format):


but skill install
Install the skill globally:


but skill install --global
Usage: but skill <COMMAND>

Subcommands
but skill install
Install the GitButler CLI skill files for Coding agents

By default, the command prompts you to choose installation scope first (current repository or global home directory), then prompts you to select a skill folder format (Agent Skills / .agents, Claude Code, OpenCode, Codex, GitHub Copilot, Cursor, Windsurf) unless you specify a custom path with --path. When run outside a git repository, local scope is unavailable and the default install location is global (home directory). You can still install to a custom location with --path using an absolute or ~ path.

Use --global to install the skill in a global location instead of the current repository.

In non-interactive mode, specify --path or --detect.

Examples
Install interactively (prompts for scope and format):


but skill install
Install globally (prompts for format):


but skill install --global
Install to a custom path:


but skill install --path .agents/skills/gitbutler
Auto-detect installation location (update existing installation):


but skill install --detect
Usage: but skill install [OPTIONS]

Options:

-g, --global — Install the skill globally instead of in the current repository
-p, --path <PATH> — Custom path where to install the skill (relative to repository root or absolute). Outside a repository, relative paths require --global
-d, --detect — Automatically detect where to install by finding existing installation
but skill check
Check if installed GitButler skills are up to date with the CLI version

Scans for installed skill files and compares their version with the current CLI version. By default, checks both local (repository) and global installations.

Examples
Check all installed skills:


but skill check
Check and automatically update outdated skills:


but skill check --update
Check only global installations:


but skill check --global
Usage: but skill check [OPTIONS]

Options:

-g, --global — Only check global installations (in home directory)
-l, --local — Only check local installations (in current repository)
-u, --update — Automatically update any outdated skills found

Commands
but config
View and manage GitButler configuration.

Without a subcommand, displays an overview of important settings including user information, target branch, forge configuration, and AI setup.

Examples
View configuration overview:


but config
View/set user configuration:


but config user
but config user set name "John Doe"
but config user set email john@example.com
View/set forge configuration:


but config forge
View/set target branch:


but config target
View/set metrics:


but config metrics
Usage: but config <COMMAND>

Subcommands
but config user
View and configure user information (name, email, editor).

Without arguments, displays current user.name, user.email, and core.editor. Use subcommands to set or unset configuration values.

Examples

View user configuration:

but config user

Set user name (locally):

but config user set name "John Doe"

Set user email globally:

but config user set --global email john@example.com

Unset a local value:

but config user unset name

Usage: but config user

but config forge
View and manage forge configuration.

Shows configured forge accounts (GitHub, GitLab, etc.) and authentication status. Use subcommands to authenticate or forget accounts.

Examples

View configured forge accounts:

but config forge

Authenticate with a forge:

but config forge auth

List authenticated accounts:

but config forge list-users

Forget an account:

but config forge forget username

Usage: but config forge

but config target
View or set the target branch.

Without arguments, displays the current target branch. With a branch name, sets the target branch.

Examples

View current target:

but config target

Set target branch:

but config target origin/main

Usage: but config target [BRANCH]

Arguments:

<BRANCH> — New target branch to set (e.g., "origin/main")
but config metrics
View or set metrics collection.

GitButler uses metrics to help us know what is useful and improve it. Privacy policy: https://gitbutler.com/privacy

Without arguments, displays the current setting.

Examples

View metrics configuration:

but config metrics

Enable metrics:

but config metrics enable

Disable metrics:

but config metrics disable

Usage: but config metrics [STATUS]

Arguments:

<STATUS> — Whether metrics are enabled
but config ui
View and configure UI preferences.

Without arguments, displays current UI settings. Use subcommands to set or unset configuration values.

Examples

View UI configuration:

but config ui

Enable TUI mode for diff by default:

but config ui set tui true

Disable TUI mode:

but config ui set tui false

Usage: but config ui

but config ai
View and configure AI provider settings.

Without subcommands, this starts an interactive setup flow. Use provider subcommands for non-interactive configuration.

Examples

Interactive setup:

but config ai

View current AI configuration:

but config ai show

Configure OpenAI non-interactively:

but config ai openai --key-option bring-your-own --api-key-env OPENAI_API_KEY --model gpt-5.4-nano

Configure Ollama locally:

but config ai --local ollama --endpoint localhost:11434 --model llama3.1

Usage: but config ai [OPTIONS]

Options:

--local — Configure local repository git config instead of global user config
--global — Configure global user git config

