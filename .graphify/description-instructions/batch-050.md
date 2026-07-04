# Node Description Batch 51 of 61

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

- "scripts_orchestrate_phase0_with_prep_rationale_97": "# TODO: Replace with actual MCP call:" | kind=entity | source=scripts/orchestrate_phase0_with_prep.py:L97 | neighbors=[orchestrate_phase0_with_prep.py]
- "scripts_orders_callbacks_split_extract": "extract()" | kind=code-symbol | source=scripts/orders_callbacks_split.py:L60 | neighbors=[orders_callbacks_split.py]
- "scripts_orders_callbacks_split_make_header": "make_header()" | kind=code-symbol | source=scripts/orders_callbacks_split.py:L42 | neighbors=[orders_callbacks_split.py]
- "scripts_orders_callbacks_split_write_file": "write_file()" | kind=code-symbol | source=scripts/orders_callbacks_split.py:L63 | neighbors=[orders_callbacks_split.py]
- "scripts_orders_management_split_extract": "extract()" | kind=code-symbol | source=scripts/orders_management_split.py:L62 | neighbors=[orders_management_split.py]
- "scripts_orders_management_split_make_header": "make_header()" | kind=code-symbol | source=scripts/orders_management_split.py:L44 | neighbors=[orders_management_split.py]
- "scripts_orders_management_split_write_file": "write_file()" | kind=code-symbol | source=scripts/orders_management_split.py:L65 | neighbors=[orders_management_split.py]
- "scripts_package_skill_rationale_28": "Check if a path should be excluded from packaging." | kind=entity | source=.bob/skills/skill-creator/scripts/package_skill.py:L28 | neighbors=[should_exclude()]
- "scripts_package_skill_rationale_43": "Package a skill folder into a .skill file.\r \r     Args:\r         skill_path: Pat" | kind=entity | source=.bob/skills/skill-creator/scripts/package_skill.py:L43 | neighbors=[package_skill()]
- "scripts_phase_0_hotspot_mcp_fastmcp_rationale_29": "Execute Phase 0 (Hotspot Analysis) for an epic.\r     \r     Args:\r         epic_i" | kind=entity | source=scripts/phase_0_hotspot_mcp_fastmcp.py:L29 | neighbors=[execute_phase_0()]
- "scripts_phase_0_hotspot_mcp_main": "main()" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp.py:L80 | neighbors=[phase_0_hotspot_mcp.py]
- "scripts_phase_0_hotspot_mcp_rationale_25": "List available MCP tools" | kind=entity | source=scripts/phase_0_hotspot_mcp.py:L25 | neighbors=[list_tools()]
- "scripts_phase_0_hotspot_mcp_rationale_47": "Handle MCP tool calls" | kind=entity | source=scripts/phase_0_hotspot_mcp.py:L47 | neighbors=[call_tool()]
- "scripts_phase_0_hotspot_mcp_rationale_54": "Return context immediately - no blocking operations" | kind=entity | source=scripts/phase_0_hotspot_mcp.py:L54 | neighbors=[execute_phase_0_tool()]
- "scripts_phase_1_5_boundary_mcp_rationale_14": "Execute Phase 1.5 (Scope Boundary Validation) for an epic.\r     Validates that p" | kind=entity | source=scripts/phase_1_5_boundary_mcp.py:L14 | neighbors=[execute_phase_1_5()]
- "scripts_phase_1_scope_mcp_fastmcp_rationale_15": "Execute Phase 1 (Scope Definition) for an epic.\r     Reads Phase 0 hotspot analy" | kind=entity | source=scripts/phase_1_scope_mcp_fastmcp.py:L15 | neighbors=[execute_phase_1()]
- "scripts_phase_1_scope_mcp_main": "main()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L298 | neighbors=[phase_1_scope_mcp.py]
- "scripts_phase_1_scope_mcp_rationale_172": "Create scope document for no-action epic." | kind=entity | source=scripts/phase_1_scope_mcp.py:L172 | neighbors=[create_no_action_scope()]
- "scripts_phase_1_scope_mcp_rationale_235": "Create scope document for extraction epic." | kind=entity | source=scripts/phase_1_scope_mcp.py:L235 | neighbors=[create_extraction_scope()]
- "scripts_phase_1_scope_mcp_rationale_28": "List available Phase 1 tools." | kind=entity | source=scripts/phase_1_scope_mcp.py:L28 | neighbors=[list_tools()]
- "scripts_phase_1_scope_mcp_rationale_60": "Execute Phase 1: Scope Definition." | kind=entity | source=scripts/phase_1_scope_mcp.py:L60 | neighbors=[execute_phase_1_tool()]
- "scripts_phase_2_architecture_mcp_rationale_14": "Execute Phase 2 (Architecture Planning) for an epic.\r     Creates detailed extra" | kind=entity | source=scripts/phase_2_architecture_mcp.py:L14 | neighbors=[execute_phase_2()]
- "scripts_phase_3_audit_mcp_rationale_14": "Execute Phase 3 (DNA & PR Audit) for an epic.\r     Runs V12 DNA compliance check" | kind=entity | source=scripts/phase_3_audit_mcp.py:L14 | neighbors=[execute_phase_3()]
- "scripts_phase_4_5_ticket_review_mcp_rationale_117": "Execute Phase 4.5 (Ticket Review) for an epic.\r     Validates tickets against Ja" | kind=entity | source=scripts/phase_4_5_ticket_review_mcp.py:L117 | neighbors=[execute_phase_4_5()]
- "scripts_phase_4_5_ticket_review_mcp_rationale_22": "Initialize Firebase using local service account credentials." | kind=entity | source=scripts/phase_4_5_ticket_review_mcp.py:L22 | neighbors=[init_firestore()]
- "scripts_phase_4_5_ticket_review_mcp_rationale_38": "Query Jane Street KB for extraction patterns and validation rules." | kind=entity | source=scripts/phase_4_5_ticket_review_mcp.py:L38 | neighbors=[query_jane_street_kb()]
- "scripts_phase_4_5_ticket_review_mcp_rationale_72": "Validate that tickets respect single-method boundary." | kind=entity | source=scripts/phase_4_5_ticket_review_mcp.py:L72 | neighbors=[validate_ticket_scope()]
- "scripts_phase_4_5_ticket_review_mcp_rationale_97": "Validate that complexity targets are realistic (CYC ≤ 8)." | kind=entity | source=scripts/phase_4_5_ticket_review_mcp.py:L97 | neighbors=[validate_complexity_targets()]
- "scripts_phase_4_tickets_mcp_rationale_14": "Execute Phase 4 (Ticket Generation) for an epic.\r     Uses jCodemunch to analyze" | kind=entity | source=scripts/phase_4_tickets_mcp.py:L14 | neighbors=[execute_phase_4()]
- "scripts_phase_5_execute_mcp_rationale_14": "Execute Phase 5 (Ticket Execution) for an epic.\r     Delegates to Bob CLI (v12-e" | kind=entity | source=scripts/phase_5_execute_mcp.py:L14 | neighbors=[execute_phase_5()]
- "scripts_phase_5_verify_mcp_rationale_14": "Execute Phase 5.V (Verification) for an epic.\r     Verifies that ticket executio" | kind=entity | source=scripts/phase_5_verify_mcp.py:L14 | neighbors=[execute_phase_5_verify()]
- "scripts_phase_6_review_mcp_rationale_14": "Execute Phase 6 (Final Review) for an epic.\r     Performs final review, generate" | kind=entity | source=scripts/phase_6_review_mcp.py:L14 | neighbors=[execute_phase_6()]
- "scripts_precompute_wave7_graph_rationale_40": "Run complexity_audit.py and return dict keyed by method_name." | kind=entity | source=scripts/precompute_wave7_graph.py:L40 | neighbors=[run_complexity_audit()]
- "scripts_precompute_wave7_graph_rationale_62": "Read all 14 OKF .md files and write a single cache JSON." | kind=entity | source=scripts/precompute_wave7_graph.py:L62 | neighbors=[build_okf_cache()]
- "scripts_precompute_wave7_graph_rationale_86": "Build precomputed.json for one epic. Returns True if written." | kind=entity | source=scripts/precompute_wave7_graph.py:L86 | neighbors=[build_precomputed()]
- "scripts_preflight_validation_rationale_118": "Detect if epic is already complete with clean execution." | kind=entity | source=scripts/preflight_validation.py:L118 | neighbors=[detect_already_complete()]
- "scripts_preflight_validation_rationale_148": "Run all special case detections before starting epic." | kind=entity | source=scripts/preflight_validation.py:L148 | neighbors=[preflight_validation()]
- "scripts_preflight_validation_rationale_195": "Validate a single epic from roadmap." | kind=entity | source=scripts/preflight_validation.py:L195 | neighbors=[validate_epic()]
- "scripts_preflight_validation_rationale_217": "Validate all epics in roadmap." | kind=entity | source=scripts/preflight_validation.py:L217 | neighbors=[validate_all_epics()]
- "scripts_preflight_validation_rationale_261": "Generate markdown report from validation results." | kind=entity | source=scripts/preflight_validation.py:L261 | neighbors=[generate_report()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-050.json

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
