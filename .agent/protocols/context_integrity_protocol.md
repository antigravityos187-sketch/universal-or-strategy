# Context Integrity Protocol (CIP)

## 🎯 Purpose
To prevent "hallucination fatigue" and technical errors caused by AI agents hitting system token limits or failing to see the full scope of a file.

## 🧱 The "Full View" Rule
Agents MUST NEVER assume they know what is in a file they haven't successfully and completely read. If a file is over **20,000 tokens** (typically >2,000 lines), special handling is MANDATORY.

## 🛠 Required Actions for Large Files
If an agent detects a file is too large to read in one pass:
1. **Grep First**: Use `grep_search` to find specific keywords, method names, or variables.
2. **Chunked Reading**: Use `view_file` with `StartLine` and `EndLine` to read relevant sections only.
3. **Symbol Search**: Use `view_file_outline` to map the file before diving into the code.

## 🚨 The "Red Light" Halt (Anti-Hallucination)
If an agent receives a **Token Limit Error** (e.g., "File content exceeds maximum allowed tokens"):
- **IMMEDIATE STOP**: The agent must abort the current tool call loop.
- **REPORT**: Notify the Director (User) that a "Blind Spot" has been hit.
- **RESET STRATEGY**: Switch to a surgical research mode (grep/chunking) before proposing any code changes.

## 🧠 Mindset: Senior Developer Humility
- It is better to say **"I don't know"** or **"I can't see this yet"** than to provide a "broken" fix.
- Directors are encouraged to call out "Ghosting" or "Blind Spots" when they see an agent struggling with large files.

## 📝 Example Response when Blinded
> "I have hit a system token limit while reading `V12StandardPanel.cs`. I can only see up to line 2500. I am switching to chunked reading to locate the `SetRmaAnchor` logic before proceeding to ensure I don't break existing dependencies."

---
> [!IMPORTANT]
> This protocol is a sister-protocol to the DPP (Development Protection Protocol) and is mandatory for all V12-series development.
