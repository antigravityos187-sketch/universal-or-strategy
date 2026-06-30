# EPIC-W7-077 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `ProcessClientStream` | **Source:** `src/V12_002.UI.IPC.Server.cs`
**Baseline CYC:** 7 | **Target CYC:** ≤ 8
**ticket_count:** 5

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ProcessClientStream_ReadChunk` (formalise) | 0 | 2 |
| T2 | `ProcessClientStream_DecodeUtf8` (formalise) | 0 | 2 |
| T3 | `ProcessClientStream_ExtractLines` (reduce) | 2 | 3 |
| T4 | `ProcessClientStream_DispatchLine` (formalise) | 0 | 1 |
| T5 | `ProcessClientStream_CheckBufferOverflow` (new) | 2 | 2 |

**projected_parent_cyc_after_all: 7**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ProcessClientStream_ReadChunk`
- **concern:** I/O poll — calls `stream.Read`, guards `bytesRead < 0` (continue) and `== 0` (disconnect) with 50ms sleep on no-data. Formalise existing extraction.
- **lines_to_move:** Already extracted — verify existing method, confirm CYC=2, confirm no lock() blocks
- **cyc_reduction:** 0 (already extracted)
- **projected_helper_cyc:** 2

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `ProcessClientStream_DecodeUtf8`
- **concern:** UTF-8 decode — strict decoder with `_ipcInvalidUtf8Count` increment on failure, returns bool success + out string chunk. Formalise existing extraction.
- **lines_to_move:** Already extracted — verify existing method, confirm CYC=2, no lock() blocks
- **cyc_reduction:** 0 (already extracted)
- **projected_helper_cyc:** 2

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `ProcessClientStream_ExtractLines`
- **concern:** Newline framing — splits StringBuilder on newline, guards buffer overflow via extracted helper, returns string[] or null for partial frames
- **lines_to_move:** Refactor existing ExtractLines to delegate double overflow check to ProcessClientStream_CheckBufferOverflow (T5), reducing CYC from 5 to 3
- **cyc_reduction:** 2
- **projected_helper_cyc:** 3

## Ticket T4

- **ticket_id:** T4
- **helper_name:** `ProcessClientStream_DispatchLine`
- **concern:** Thin routing shim — delegates single string line to `HandleIncomingIpcLine(session, line)`. Formalise existing extraction.
- **lines_to_move:** Already extracted — verify existing method, confirm CYC=1
- **cyc_reduction:** 0 (already extracted)
- **projected_helper_cyc:** 1

## Ticket T5

- **ticket_id:** T5
- **helper_name:** `ProcessClientStream_CheckBufferOverflow`
- **concern:** Buffer overflow guard — evaluates `lineBuffer.Length > IpcMaxBufferedChars`, logs telemetry, sets disconnectClient=true. Extracted from ExtractLines to reduce its CYC from ~5 to ~3.
- **lines_to_move:** Dual overflow check from `ProcessClientStream_ExtractLines` body
- **cyc_reduction:** 2 (from ExtractLines)
- **projected_helper_cyc:** 2

---

## projected_parent_cyc_after_all: 7

Parent `ProcessClientStream` retains: while + 2 if-continue/break + 1 if-break on decode + 2 if-break/continue on lines + 1 foreach. CYC = 7, compliant.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-077 |
