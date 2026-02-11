# Universal OR Strategy V12.11 - Baseline Architecture DNA

This document serves as the high-density knowledge base for the Universal OR Strategy (V12.11). It is designed for AI memory systems to provide instant, accurate context on the trading system across different IDEs.

## 1. Core Architecture: SIMA (Single-Instance Multi-Account)
- **Concept:** A single NinjaScript strategy instance runs on a "Master" account and broadcasts its state/orders to a "Fleet" of follow accounts (primarily Apex Trader Funding).
- **Dispatch Engine:** Orders are submitted to the fleet via `ExecuteSmartDispatchEntry`.
- **Account Filtering:** Uses `AccountPrefix` (standard: "Apex") to identify fleet members.
- **Circuit Breaker:** Logic in `OnStateChange` and `OnBarUpdate` to prevent order cascades during high-volatility.

## 2. Execution Logic & Modes
- **ORB (Opening Range Breakout):** Original WSGTA logic. Tracks High/Low/Mid of a specified timeframe (e.g., 5-min or 15-min).
- **RMA (Risk Managed Account) Mode:** 
  - Dynamic position sizing based on ATR.
  - Bracket submission deferred until entry fill to ensure price parity across accounts.
  - Anchors: EMA30, EMA65, EMA200, ORH, ORL.
- **CIT (Chase-If-Touch):** If price touches a limit order but doesn't fill, the strategy dynamically adjusts the limit price by `ChaseIfTouchPoints` to secure the entry.
- **RMA 9/15 Split:** In TREND/RMA mode, the position is split between EMA9 and EMA15 anchors.

## 3. Position & Target Management (V12 50/50 Rule)
- **Dual Profit Targets:**
  - **T1:** 50% of position (fixed points or ATR-based).
  - **T2:** 50% of position (ATR-based runner).
- **Stop Management:**
  - **Breakeven (BE):** Triggers after T1 is hit.
  - **Trailing:** Dynamic ATR-based trailing or EMA-trailing (for RETEST/TREND).
- **Reaper Audit:** A background thread (`ReaperAudit`) runs every 1000ms to verify that fleet account positions match the Master instance. It auto-flattens desynced accounts if enabled.

## 4. UI Design System (Standard Edition)
- **Primary Palette:**
  - Background: #050508 (Deep Black/Blue)
  - UI Accents: #22d3ee (Cyan)
  - Buy: #064e3b / Sell: #7f1d1d
- **Panel Modes:**
  - **ORB/RMA/RETEST/MOMO/FFMA/TREND:** Six distinct operational modes each with persisted settings (Stop types, Target counts, Max Risk).
  - **Telemetry:** Real-time display of EMAs, OR levels, and Fleet P/L.

## 5. Compliance Hub
- **Consistency Lock:** Monitors profit contribution to ensure no single day exceeds the Apex 30% rule.
- **Daily Profit Cap:** Guards the account from "over-trading" once a target is reached.
