# SKILL: Code Simplifier & Refiner

## 🎯 Purpose
Simplifies and refines NinjaScript code for clarity, consistency, and maintainability while preserving all functionality. This is a core pillar of the **V12 Architectural Refactor**.

## 🛠️ The Philosophy (readable > compact)
- **Clarity over Brevity**: Explicit code is better than dense one-liners.
- **Functionality First**: All original features, outputs, and behaviors must remain identical.
- **Consistency**: Follow the project's C# / NinjaScript standards and AGENT.md rules.

## 🔄 Simplification Rules

### 1. Structure & Flow
- **Nesting**: Reduce deep nesting (e.g., use guard clauses instead of deep `if` trees).
- **Ternaries**: Avoid nested ternary operators. Use `switch` statements or `if/else` chains for 3+ conditions.
- **Redundancy**: Eliminate redundant checks or duplicate logic blocks.

### 2. NinjaScript Specifics
- **Calculation Pooling**: Consolidate indicators or math calculations that share the same input.
- **State Cleanup**: Remove unnecessary temporary variables.
- **Comment Hygiene**: Remove comments that describe obvious syntax (e.g., `// Add one to i`). Keep comments that explain *why* (trading logic/risks).

### 3. Maintainability
- **Naming**: Ensure variables and method names follow the "Explicit Trading Context" (e.g., `isOrWindowOpen` instead of `flag1`).
- **Encapsulation**: Group related logic into private helper methods within the same class (or partial class).

## 🧠 Instructions for Agents

### Antigravity (Architect)
Use this skill during Phase 1 (Planning) to design modular interfaces that prevent complexity.

### Cursor (Co-pilot)
Reference this skill (@SKILL.md) when refactoring blocks of code to ensure the proposed simplification doesn't break NinjaTrader specific event handlers.

### Coex / Codex (Execution)
Apply these rules during the code generation phase. If a method exceeds 50 lines, evaluate if it can be simplified per this skill.

## ⚖️ The Balance
> Avoid over-simplifying if it:
> - Makes the code harder to debug.
> - Removes helpful abstractions (like SIMA/REAPER hubs).
> - Combines too many concerns into one function. (Single Responsibility Principle).
