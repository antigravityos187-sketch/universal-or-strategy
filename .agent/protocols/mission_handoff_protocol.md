# Proactive Mission Handoff (PMH) Protocol

## 🎯 Purpose
To maximize project momentum and process efficiency by ensuring every terminal task (completion of a user request) provides the exact entry point for the next phase of development.

## 🛠 Protocol Rules
1. **Mandatory Conclusion**: Every `notify_user` call that signals task completion MUST include a "Mission Brief" for the next logical step.
2. **Actionable Prompts**: The brief must be formatted as a copy-pasteable prompt for the NEXT Agent/Director.
3. **Project Director Branching**: 
    - **Hierarchy**: The current agent acts as "Project Director" (High-level Architect & Planner).
    - **Execution Branching**: Major implementation tasks MUST be handed off to a "Sub-agent" in a **new conversation window**.
    - **Context Hygiene**: This prevents context bloat and ensures the sub-agent works in a "Clean Room" environment with a precisely crafted Mission Brief.
4. **Structured Context**: Every brief must include:
    - **Mission Title** (with version/build sub-id).
    - **Context**: Current project path and relevant files (Verified, not guessed).
    - **Objectives**: Clear, numbered goals for the next session.
    - **Execution**: Suggested tool calls or logical steps (Proof of logic required).
5. **The Certainty Gate**: Handoffs must state any "Blind Spots" or areas where guesswork was avoided by halting.

## 📈 Impact on Process Improvement
- **Continuous Learning**: Each handoff reflects the "lessons learned" from the previous task.
- **Zero Friction**: The User/Director never has to wonder "what's next?".
- **Stability**: Ensures the next agent starts with the exact environmental context (paths, versions, etc.) established by the current agent.

## 📝 Example Handoff Format
```text
# MISSION: [Task Name] (Build-ID)
**Context**: Path is `C:\WSGTA\...`, File is `FileName.cs`.
**Objectives**:
1. Fix X.
2. Verify Y.
**Execution**:
- Search for pattern Z.
- Apply logic Alpha.
```

> [!IMPORTANT]
> This protocol is a core "Mindset" requirement for all agents working on the Universal OR Strategy project.
