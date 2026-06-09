MCP
MCP Overview
Use Greptile’s MCP server to access code review tools directly in Claude, Cursor, VS Code, or Codex CLI. Fetch comments, apply fixes, and manage coding patterns.

The Greptile MCP server lets AI coding assistants (Claude, Cursor, Copilot, Codex) access your code review data directly. Instead of switching to GitHub or the Greptile dashboard, you can fetch comments, apply fixes, and manage coding patterns from your editor.
​
What You Can Do
Fetch PR comments - Get unaddressed Greptile feedback for any PR
Apply suggested fixes - Comments often include code suggestions you can apply directly
Search feedback patterns - Find recurring issues across all your reviews
Manage coding standards - View and create your team’s custom context patterns
Check review status - See which comments are addressed before merging

MCP
Custom Context
View, search, and create coding standards from your IDE using Greptile’s MCP tools. Turn recurring review feedback into enforceable team patterns.

Custom context refers to your team’s coding standards that Greptile checks during reviews. Rules like “use async/await instead of promises” or “API endpoints must validate input.” When code doesn’t follow a pattern, Greptile comments on the PR.
With MCP, you can view, search, and create patterns from your IDE.
​
View Your Patterns
What coding patterns does my organization have?
Tool used: list_custom_context
List org coding standards with mcp
​
Search Patterns
Search our coding patterns for error handling
Tool used: search_custom_context
​
Get Pattern Details
Show details for pattern 9c29e7ed-2d3f-45bd-846d-a61a59f10dd9
Tool used: get_custom_context
Returns the full pattern including linkedComments—PRs where this pattern triggered feedback.
​
Create a Pattern
Create a coding pattern: "All React components must have TypeScript interfaces for props"
Apply it to .tsx files only.
Tool used: create_custom_context
Create custom context using the MCP
​
Scope Examples
You Say	Pattern Applies To
”Apply everywhere”	All files in all repos
”Apply to TypeScript files”	**/*.ts
”Apply to the api folder”	**/api/**
”Apply to owner/repo only”	That specific repository
​
Disable a Pattern
There’s no delete. Set status to inactive:
Disable the pattern about console.log statements
​
Workflow: Turn Recurring Feedback Into a Pattern
When you notice Greptile making the same comment repeatedly:
1
Identify the pattern

Search Greptile comments for "error handling"
Find comments that keep appearing across PRs.
2
Create the custom context

Create a pattern: "All catch blocks must log the error before re-throwing"
Apply to all TypeScript files.
3
Verify it's active

List my custom contexts and confirm the new pattern is ACTIVE
​
Field Reference
Field	Description
body	The rule text
type	CUSTOM_INSTRUCTION (explicit rule) or PATTERN (code pattern)
status	ACTIVE, INACTIVE, or SUGGESTED
scopes	Where it applies (see tools reference for format)
commentsCount	Times this pattern triggered a comment
linkedComments	PRs where this pattern was applied

MCP
Tools Reference
Complete API reference for Greptile’s MCP tools. Documentation for PR management, code reviews, comment search, and custom context endpoints with examples.

Complete reference for all tools provided by the Greptile MCP server.
Repository parameters (name, remote, defaultBranch) must be provided together or omitted entirely.
​
Pull Request Tools
​
list_pull_requests / list_merge_requests
List PRs with optional filtering. Both tool names work identically.
Parameters
Response
Parameter	Type	Required	Description
name	string	No*	Repository name (owner/repo)
remote	string	No*	github, gitlab, azure, bitbucket
defaultBranch	string	No*	Default branch name
sourceBranch	string	No	Filter by source branch (partial match)
authorLogin	string	No	Filter by author (fuzzy match)
state	string	No	open, closed
limit	number	No	Max results (default: 20, max: 100)
offset	number	No	Pagination offset
Merged PRs also appear under state: "closed".
​
get_merge_request
Get detailed PR information including review analysis.
Parameters
Response
Parameter	Type	Required	Description
name	string	Yes	Repository name (owner/repo)
remote	string	Yes	github, gitlab, azure, bitbucket
defaultBranch	string	Yes	Default branch
prNumber	number	Yes	PR number
​
list_merge_request_comments
Get all comments for a PR with filtering options.
Parameters
Response
Parameter	Type	Required	Description
name	string	Yes	Repository name
remote	string	Yes	Provider
defaultBranch	string	Yes	Default branch
prNumber	number	Yes	PR number
greptileGenerated	boolean	No	Filter Greptile comments only
addressed	boolean	No	Filter by addressed status
createdAfter	string	No	ISO 8601 date filter
createdBefore	string	No	ISO 8601 date filter
Two Greptile identities: PR summaries come from greptile-apps[bot], inline comments from greptile-apps. Use isGreptileComment: true to catch both.
​
Code Review Tools
​
list_code_reviews
List code reviews with optional filtering.
Parameters
Response
Parameter	Type	Required	Description
name	string	No	Repository name
remote	string	No	Provider
defaultBranch	string	No	Default branch
prNumber	number	No	Filter by PR
status	string	No	Filter by status
limit	number	No	Max results (default: 20)
offset	number	No	Pagination offset
Status values: PENDING, REVIEWING_FILES, GENERATING_SUMMARY, COMPLETED, FAILED, SKIPPED
​
get_code_review
Get detailed information for a specific code review.
Parameters
Response
Parameter	Type	Required	Description
codeReviewId	string	Yes	Code review ID
​
trigger_code_review
Start a new code review on a PR.
Parameters
Response
Parameter	Type	Required	Description
name	string	Yes	Repository name
remote	string	Yes	Provider
defaultBranch	string	Yes	Default branch
prNumber	number	Yes	PR number
branch	string	No	Working branch
defaultBranch is required despite appearing optional. Omitting it returns: MCP error -32000: invalid_type - defaultBranch Required
​
Comment Search Tool
​
search_greptile_comments
Search across all Greptile comments.
Parameters
Response
Parameter	Type	Required	Description
query	string	Yes	Search term
limit	number	No	Max results (default: 10, max: 50)
includeAddressed	boolean	No	Include resolved comments (default: false)
createdAfter	string	No	ISO 8601 date filter
​
Custom Context Tools
​
list_custom_context
List your organization’s coding patterns.
Parameters
Response
Parameter	Type	Required	Description
type	string	No	CUSTOM_INSTRUCTION or PATTERN
greptileGenerated	boolean	No	Filter by source
limit	number	No	Max results (default: 20, max: 100)
offset	number	No	Pagination offset
​
get_custom_context
Get details for a specific pattern.
Parameters
Response
Parameter	Type	Required	Description
customContextId	string	Yes	UUID of the context
​
search_custom_context
Search patterns by content.
Parameters
Response
Parameter	Type	Required	Description
query	string	Yes	Search term
limit	number	No	Max results (default: 10, max: 50)
​
create_custom_context
Create a new coding pattern.
Parameters
Response
Parameter	Type	Required	Description
body	string	No	Pattern content
type	string	No	CUSTOM_INSTRUCTION or PATTERN
status	string	No	ACTIVE, INACTIVE, SUGGESTED
scopes	object	No	Where pattern applies
metadata	object	No	Additional data
Scope structure:
{
  "AND": [
    {
      "operator": "MATCHES",
      "field": "filepath",
      "value": "**/api/**"
    }
  ]
}
There’s no delete_custom_context tool. To disable a pattern, set status: "INACTIVE".
​
Error Handling
Standard JSON-RPC error format:
{
  "jsonrpc": "2.0",
  "id": 1,
  "error": {
    "code": -32601,
    "message": "Method not found"
  }
}
Common Error Codes:
Code	Meaning
-32700	Parse error
-32600	Invalid request
-32601	Method not found
-32602	Invalid parameters
-32603	Internal error
-32000	Server error (includes auth failures)


