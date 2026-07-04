# Node Description Batch 50 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
For a code symbol (kind=code-symbol — a function, class, or constant),
describe what the function/symbol does based on its name, source location
and neighbors — e.g. "Resolves the configured ontology profile from graphify.yaml.".
For an entity node (any other kind — e.g. a person, place, event, object),
describe what the entity is and its role, grounded in its type, its
relations (neighbors) and the provided citations/evidence — e.g.
"Lady Carfax, a wealthy heiress who disappears en route to Lausanne.".
Ground entity descriptions in the citations/evidence when present; do not
speculate beyond the context, so a node with no supporting context may be
left out of the reply.
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "scripts_monitor_vm_progress_rationale_53": "Execute command on VM via gcloud SSH." | kind=entity | source=scripts/monitor_vm_progress.py:L53 | neighbors=[run_ssh_command()]
- "scripts_monitor_vm_progress_rationale_73": "Check if VM is running and accessible." | kind=entity | source=scripts/monitor_vm_progress.py:L73 | neighbors=[check_vm_status()]
- "scripts_monitor_vm_progress_rationale_78": "Get epic status from manifest.json on VM." | kind=entity | source=scripts/monitor_vm_progress.py:L78 | neighbors=[get_epic_status()]
- "scripts_negative_evidence_check_negativeevidencecache_init": ".__init__()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L26 | neighbors=[NegativeEvidenceCache]
- "scripts_negative_evidence_check_rationale_24": "Manages cache of failed searches." | kind=entity | source=scripts/negative_evidence_check.py:L24 | neighbors=[NegativeEvidenceCache]
- "scripts_negative_evidence_check_rationale_45": "Check if query has negative evidence. Returns evidence entry if found." | kind=entity | source=scripts/negative_evidence_check.py:L45 | neighbors=[.check()]
- "scripts_negative_evidence_check_rationale_58": "Record negative evidence for a query." | kind=entity | source=scripts/negative_evidence_check.py:L58 | neighbors=[.record()]
- "scripts_negative_evidence_check_rationale_80": "List all negative evidence entries." | kind=entity | source=scripts/negative_evidence_check.py:L80 | neighbors=[.list_all()]
- "scripts_negative_evidence_check_rationale_99": "Clear all negative evidence." | kind=entity | source=scripts/negative_evidence_check.py:L99 | neighbors=[.clear()]
- "scripts_nexus_relay_rationale_13": "Formalizes the handoff to a sub-agent and emits a LangSmith trace." | kind=entity | source=scripts/nexus_relay.py:L13 | neighbors=[relay_to_agent()]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_init": ".__init__()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L34 | neighbors=[BobCoinBudgetManager]
- "scripts_orchestrate_full_epic_execution_rationale_109": "Orchestrates wave-based epic execution across all phases." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L109 | neighbors=[EpicWaveOrchestrator]
- "scripts_orchestrate_full_epic_execution_rationale_124": "Get list of pending epics." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L124 | neighbors=[._get_pending_epics()]
- "scripts_orchestrate_full_epic_execution_rationale_128": "Get epics for a specific wave." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L128 | neighbors=[._get_wave_epics()]
- "scripts_orchestrate_full_epic_execution_rationale_134": "Execute a single phase for a single epic.\r         \r         In production, this" | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L134 | neighbors=[._execute_phase_for_epic()]
- "scripts_orchestrate_full_epic_execution_rationale_154": "# TODO: Replace with actual MCP tool call" | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L154 | neighbors=[orchestrate_full_epic_execution.py]
- "scripts_orchestrate_full_epic_execution_rationale_167": "Execute a single wave (all epics in wave for one phase)." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L167 | neighbors=[.execute_wave()]
- "scripts_orchestrate_full_epic_execution_rationale_234": "Execute a single phase for all waves." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L234 | neighbors=[.execute_phase_all_waves()]
- "scripts_orchestrate_full_epic_execution_rationale_272": "Execute all phases for all epics." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L272 | neighbors=[.execute_all_phases()]
- "scripts_orchestrate_full_epic_execution_rationale_291": "Print final execution summary." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L291 | neighbors=[._print_final_summary()]
- "scripts_orchestrate_full_epic_execution_rationale_32": "Manages BobCoin budget tracking and refill prompts." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L32 | neighbors=[BobCoinBudgetManager]
- "scripts_orchestrate_full_epic_execution_rationale_40": "Check current BobCoin balance.\r         \r         In production, this would quer" | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L40 | neighbors=[.check_balance()]
- "scripts_orchestrate_full_epic_execution_rationale_46": "# TODO: Integrate with actual Bob IDE balance API" | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L46 | neighbors=[orchestrate_full_epic_execution.py]
- "scripts_orchestrate_full_epic_execution_rationale_51": "Record cost for a phase/wave execution." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L51 | neighbors=[.record_cost()]
- "scripts_orchestrate_full_epic_execution_rationale_60": "Calculate average cost per epic for a specific phase." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L60 | neighbors=[.get_average_cost_per_phase()]
- "scripts_orchestrate_full_epic_execution_rationale_67": "Predict cost for next wave based on historical data." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L67 | neighbors=[.predict_wave_cost()]
- "scripts_orchestrate_full_epic_execution_rationale_74": "Check if refill is needed." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L74 | neighbors=[.needs_refill()]
- "scripts_orchestrate_full_epic_execution_rationale_82": "Prompt user to refill BobCoins." | kind=entity | source=scripts/orchestrate_full_epic_execution.py:L82 | neighbors=[.prompt_refill()]
- "scripts_orchestrate_phase_execution_phaseorchestrator_save_roadmap": "._save_roadmap()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L42 | neighbors=[PhaseOrchestrator]
- "scripts_orchestrate_phase_execution_rationale_105": "Execute a single phase for an epic" | kind=entity | source=scripts/orchestrate_phase_execution.py:L105 | neighbors=[.execute_phase()]
- "scripts_orchestrate_phase_execution_rationale_161": "Execute a phase for all ready epics in parallel" | kind=entity | source=scripts/orchestrate_phase_execution.py:L161 | neighbors=[.execute_wave()]
- "scripts_orchestrate_phase_execution_rationale_207": "Generate complete execution plan for an epic" | kind=entity | source=scripts/orchestrate_phase_execution.py:L207 | neighbors=[.generate_execution_plan()]
- "scripts_orchestrate_phase_execution_rationale_30": "Orchestrates parallel phase execution across multiple epics" | kind=entity | source=scripts/orchestrate_phase_execution.py:L30 | neighbors=[PhaseOrchestrator]
- "scripts_orchestrate_phase_execution_rationale_55": "Get all epics ready for a specific phase" | kind=entity | source=scripts/orchestrate_phase_execution.py:L55 | neighbors=[.get_epics_by_phase()]
- "scripts_orchestrate_phase_execution_rationale_77": "Check if epic is ready for a specific phase" | kind=entity | source=scripts/orchestrate_phase_execution.py:L77 | neighbors=[._is_ready_for_phase()]
- "scripts_orchestrate_phase0_with_prep_rationale_121": "Execute Phase 0 with orchestrator-level preparation.\r     \r     This is the main" | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L121 | neighbors=[execute_phase0_with_prep()]
- "scripts_orchestrate_phase0_with_prep_rationale_163": "Main entry point for CLI usage." | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L163 | neighbors=[main()]
- "scripts_orchestrate_phase0_with_prep_rationale_25": "Pre-fetch jCodemunch data for an epic method.\r     \r     This runs BEFORE callin" | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L25 | neighbors=[prepare_jcodemunch_data()]
- "scripts_orchestrate_phase0_with_prep_rationale_45": "# TODO: Replace with actual jCodemunch MCP call:" | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L45 | neighbors=[orchestrate_phase0_with_prep.py]
- "scripts_orchestrate_phase0_with_prep_rationale_82": "Call Phase 0 MCP server with pre-fetched data.\r     \r     The Phase 0 server rec" | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L82 | neighbors=[call_phase0_mcp()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-049.json

Keep each description factual and concise (one sentence). No markdown, no prose
outside the JSON object. It is acceptable to omit a node if context is
insufficient — but include every node you can ground confidently.

Example answer format:
```json
{
  "node_id_1": "Resolves the configured ontology profile from graphify.yaml.",
  "node_id_2": "Colonel James Barclay, an antagonist in The Crooked Man."
}
```
