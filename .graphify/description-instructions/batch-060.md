# Node Description Batch 61 of 61

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

- "wave7_generate_phase0_scripts_rationale_19": "Load pending epics from epic_roadmap_wave7.json (all 161 epics)." | kind=entity | source=scripts/wave7/generate_phase0_scripts.py:L19 | neighbors=[load_pending_epics()]
- "wave7_generate_phase0_scripts_rationale_204": "Generate Phase 0 scripts for all pending Wave 7 epics with API rotation." | kind=entity | source=scripts/wave7/generate_phase0_scripts.py:L204 | neighbors=[generate_scripts()]
- "wave7_generate_phase0_scripts_rationale_49": "Load all 15 API keys from JSON files." | kind=entity | source=scripts/wave7/generate_phase0_scripts.py:L49 | neighbors=[load_api_keys()]
- "wave7_identify_phase0_complete_rationale_11": "Find all EPIC-W7-* directories with 00-hotspots.md" | kind=entity | source=scripts/wave7/identify_phase0_complete.py:L11 | neighbors=[find_phase0_complete_epics()]
- "wave7_launch_epic_with_fixed_env_rationale_22": "Create a fixed environment with proper PATH.          Returns:         dict: Env" | kind=entity | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L22 | neighbors=[get_fixed_environment()]
- "wave7_launch_epic_with_fixed_env_rationale_53": "Launch an epic script with fixed environment.          Args:         script_path" | kind=entity | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L53 | neighbors=[launch_epic()]
- "wave7_launch_epic_with_fixed_env_rationale_85": "Launch multiple epics in parallel with staggered start times.          Args:" | kind=entity | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L85 | neighbors=[launch_epic_batch()]
- "home_malhitticrypto_universal_or_strategy_linting_csproj": "Linting" | kind=code-symbol | source=Linting.csproj
- "home_malhitticrypto_universal_or_strategy_testing_csproj": "Testing" | kind=code-symbol | source=Testing.csproj
- "home_malhitticrypto_universal_or_strategy_tests_v12_performance_tests_v12_performance_tests_csproj": "V12_Performance.Tests.csproj" | kind=code-symbol | source=benchmarks/V12_Performance.Benchmarks.csproj
- "scripts_dead_code_scan": "dead_code_scan.py" | kind=code-symbol | source=scripts/dead_code_scan.py:L1

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-060.json

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
