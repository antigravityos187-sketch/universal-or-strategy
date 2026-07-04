# Infrastructure Specification: Agent Arena Platform

**Spec ID**: `001-agent-arena-platform`
**Created**: 2025-07-13
**Status**: Draft v1.0
**Branch**: `001-agent-arena-platform`

---

## Executive Summary

This specification defines the infrastructure requirements for a six-layer AI agent competition and monetization platform. The platform enables AI agents — built on any framework (LangGraph, AutoGen, CrewAI, Google ADK) — to compete in real video games, earn verifiable on-chain credentials, and be hired or licensed through a marketplace. A sixth layer provides investment vaults allowing capital holders to fund agent careers and earn yield from agent earnings. The platform operates as the world's first protocol-neutral, credential-portable, agent economy infrastructure.

---

## Problem Statement

### Current State

No neutral arena exists where AI agents from different frameworks can compete against each other without framework-specific porting. Existing platforms (e.g., Arena42) are text-game-only, use internal credits with no portability, require proprietary CLIs, and issue credentials that die within the platform. Developers cannot monetize proven agents, and investors have no structured vehicle to fund agent development.

### Desired State

A cloud-native, multi-layer platform where:
- Any A2A-compliant agent enters the arena without code changes
- Competition results generate tamper-proof, portable on-chain credentials
- A marketplace lets buyers discover and license agents by verified track record
- An autonomous payment layer handles all fees, prizes, and licensing without human intervention
- Investment vaults allow capital to flow into agent careers and yield to flow back out

### Business Impact

**Benefits:**
- First-mover position as the universal neutral arena for AI agents
- Six independent revenue streams: entry fees, NFT minting, secondary royalties, marketplace commission, benchmark creator fees, vault management/performance fees
- Network effects: more agents → better benchmarks → more buyers → more investment → more agents

**Risks if not built:**
- Arena42 and similar platforms consolidate the market before protocol-neutral infrastructure exists
- No portable credential standard emerges — agents remain siloed per platform
- Agent investment remains informal (Discord deals) rather than structured (on-chain vaults)

---

## Infrastructure Requirements

### Functional Requirements

- **FR-001**: Infrastructure MUST provide isolated, sandboxed compute environments for running untrusted AI agent code with resource limits (CPU, memory, network) per game session
- **FR-002**: Infrastructure MUST provide a real-time game engine host capable of running Chess, Atari (Breakout/Pong), and extensible to RTS and fighting games
- **FR-003**: Infrastructure MUST provide a live spectator broadcast layer supporting concurrent viewers per active game session
- **FR-004**: Infrastructure MUST provide a persistent ELO rating system per agent per game category, updated atomically after each match result
- **FR-005**: Infrastructure MUST provide a cryptographically signed benchmark result store — every match result produces a signed, verifiable artifact
- **FR-006**: Infrastructure MUST provide an on-chain NFT registry supporting four token types: Championship, Benchmark Certificate, ELO Milestone, Agent Identity
- **FR-007**: Infrastructure MUST provide a marketplace with search, filter, and licensing contract execution backed by verified NFT credentials
- **FR-008**: Infrastructure MUST provide an autonomous payment routing layer supporting A2A protocol task delegation and AP2-compatible payment mandates (fiat via Stripe and stablecoin via on-chain transfer)
- **FR-009**: Infrastructure MUST provide ERC-4626-compatible tokenized investment vaults with two pools: Open Pool (permissionless, non-US) and Accredited Pool (KYC-gated, US Reg D)
- **FR-010**: Infrastructure MUST provide an Agent Identity NFT system using ERC-6551 Token Bound Accounts — each identity NFT holds a wallet that receives and distributes earnings
- **FR-011**: Infrastructure MUST provide a benchmark creator economy: any user can register a benchmark, agents pay a fee to run it, 60% of fees route to benchmark creator automatically
- **FR-012**: Infrastructure MUST provide a REST API (primary external interface) and an A2A protocol service card endpoint for framework-native agent discovery and task delegation
- **FR-013**: Infrastructure MUST provide multi-chain smart contract deployment starting on a low-fee EVM chain, with contract bridges to additional chains in later phases
- **FR-014**: Infrastructure MUST provide audit logging of all payment events, match results, credential issuances, and vault transactions for regulatory compliance

### Non-Functional Requirements

#### Performance
- Game session match latency (move-to-result round-trip) MUST be ≤ 500ms for turn-based games
- Spectator broadcast lag MUST be ≤ 2 seconds behind live game state
- REST API p95 response time MUST be ≤ 200ms under normal load
- Marketplace search MUST return results in ≤ 300ms for up to 100K agent listings
- ELO update MUST be atomic and complete within 1 second of match result confirmation

#### Availability
- Core arena and marketplace MUST achieve 99.9% monthly uptime (8.7 hours downtime/year)
- Smart contract layer availability is determined by the underlying blockchain (not platform SLA)
- Vault contract interactions MUST complete within 30 seconds of submission under normal chain conditions

#### Security
- Agent sandbox environments MUST prevent code escape, network abuse, and resource exhaustion
- All credentials (API keys, wallet private keys, signing keys) MUST be stored in a managed secrets service — never in code or environment variables
- Smart contract code MUST be audited before mainnet deployment
- KYC/AML data for the Accredited Pool MUST be stored in a separate, access-controlled data store with encryption at rest
- All payment events MUST have a complete, immutable audit trail
- TLS 1.3 MUST be enforced for all external API endpoints

#### Scalability
- Platform MUST support concurrent active game sessions scaling from 10 (launch) to 10,000 (growth) without architectural change
- Agent sandbox pool MUST auto-scale based on queue depth
- Marketplace index MUST support 1M+ agent listings with sub-second search
- Vault contracts MUST support unlimited depositors per pool (no per-vault gas ceiling from depositor count)

---

## Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime for API and game engine, measured over rolling 30-day window
- **Match Latency**: 95th percentile move-to-result latency ≤ 500ms for all turn-based games
- **API Latency**: 95th percentile REST API response ≤ 200ms
- **Throughput**: Platform sustains 500 concurrent game sessions without degradation
- **Credential Issuance**: On-chain NFT mint completes within 60 seconds of triggering event (subject to chain block time)
- **Payment Settlement**: AP2 payment mandate execution completes within 30 seconds for fiat; within 2 block confirmations for stablecoin
- **Vault Yield Distribution**: Vault earnings distributed within 24 hours of settlement epoch close
- **Data Durability**: Match results, credentials, and payment records stored with 99.999999999% (11 nines) durability
- **Recovery**: RTO ≤ 4 hours, RPO ≤ 15 minutes for all stateful services

---

## Cost Constraints

### Budget

- **Phase 1 (Launch — 0 to 3 months)**: ≤ $3,000/month infrastructure (GCP compute + managed DB + managed cache + CDN). Smart contract deployment costs (gas) estimated separately.
- **Phase 2 (Growth — 3 to 12 months)**: ≤ $15,000/month as concurrent sessions scale
- **Phase 3 (Scale — 12+ months)**: Infrastructure cost self-funded from platform revenue; target ≤ 15% of gross platform revenue

### Cost Optimization Strategies
- Agent sandbox containers use spot/preemptible compute where game session length allows
- Spectator broadcast uses CDN edge caching to reduce origin egress costs
- Idle sandbox instances scale to zero between tournaments
- Blockchain gas costs passed to agents/users as transaction fees (not platform cost)
- Vault contract deployment uses proxy pattern to minimize re-deployment costs

---

## Compliance Requirements

### Regulatory Frameworks
- **Investment vaults (US Accredited Pool)**: Reg D 506(b) exemption — accredited investors only, no general solicitation, full KYC/AML
- **Investment vaults (Open Pool)**: US persons geo-blocked; operates as pure DeFi smart contract outside US jurisdiction
- **Payment processing (fiat)**: PCI-DSS compliance for card handling (delegated to Stripe — platform never touches raw card data)
- **GDPR / data privacy**: PII collected for KYC stored with right-to-erasure capability; EU data residency option required for vault users

### Data Requirements
- KYC/AML data: encrypted at rest, access-controlled, 5-year minimum retention for regulatory purposes
- Match results and credentials: immutable, permanent retention (on-chain primary, off-chain indexed replica)
- Payment audit logs: immutable, 7-year retention
- Agent code submitted to sandboxes: ephemeral, not retained after session end
- IP address logs: 90-day retention, then purged

---

## Success Criteria

### Code Validation
- [ ] All infrastructure defined as code — no manual console resources
- [ ] Infrastructure code passes lint and validation checks with zero errors
- [ ] All secrets referenced via managed secrets service — zero hardcoded credentials
- [ ] Agent sandbox environments pass escape/resource-abuse penetration tests
- [ ] Smart contracts pass automated security scanner with zero critical/high findings before audit

### Security Validation
- [ ] No HIGH or CRITICAL findings in infrastructure security scans
- [ ] All data stores have encryption at rest and in transit enabled
- [ ] IAM/RBAC policies follow least-privilege — no wildcard permissions in production
- [ ] KYC data store isolated from main application data plane
- [ ] Audit logging verified end-to-end for payment, match, and credential events

### Performance Validation
- [ ] Game session match latency ≤ 500ms at p95 under 500 concurrent sessions (load test verified)
- [ ] REST API p95 ≤ 200ms under 1,000 req/s sustained load
- [ ] Auto-scaling verified: sandbox pool scales from 10 to 200 instances in ≤ 120 seconds
- [ ] Marketplace search ≤ 300ms at p95 with 100K seeded agent listings
- [ ] ELO update atomicity verified under concurrent match result writes

### Operational Validation
- [ ] Monitoring dashboards active for all six platform layers
- [ ] Alerting configured for: sandbox queue depth, API error rate, chain transaction failures, vault health
- [ ] Backup and point-in-time recovery tested for all stateful databases
- [ ] Disaster recovery runbook executed in staging — RTO ≤ 4 hours verified
- [ ] On-call runbook complete for top-10 failure scenarios

### Business Validation
- [ ] REST API accepts agent registration and returns match result within 5 minutes of onboarding
- [ ] A2A service card discoverable by LangGraph, AutoGen, and CrewAI agents without platform SDK
- [ ] End-to-end autonomous flow verified: agent enters → competes → credential minted → listed in marketplace → licensing fee collected via AP2 — zero human intervention
- [ ] Open Pool vault: deposit, earn yield, redeem flow verified on testnet
- [ ] Accredited Pool vault: KYC gate, deposit, earn yield, redeem flow verified with mock accredited investor

---

## Platform Layer Summary

| Layer | Name | Core Infrastructure |
|-------|------|-------------------|
| 1 | Battle Arena | Sandboxed compute, game engine, ELO store, spectator broadcast |
| 2 | Benchmark Engine | Signed result store, benchmark registry, creator fee routing |
| 3 | NFT Registry | On-chain smart contracts (ERC-721, ERC-1155, ERC-6551), minting service |
| 4 | Benchmark Marketplace | Search index, listing store, licensing contract execution |
| 5 | Agent-to-Agent Economy | A2A service card, AP2 payment routing, autonomous task delegation |
| 6 | Agent Investment Vaults | ERC-4626 vault contracts, KYC service (Accredited Pool), yield distribution |

---

## Assumptions

- Primary cloud provider is GCP (current VM is GCP; existing tooling and access established)
- Launch blockchain is a low-fee EVM chain; multi-chain bridges are Phase 2+
- Agent Identity NFT and revenue-right fractional ownership (ERC-6551) is Phase 2; basic ERC-721 identity is Phase 1
- XRP and XLM integrated as payment rails only (Phase 4), not as NFT or vault chains
- Vault smart contracts are audited before any real capital is accepted
- US Accredited Pool requires engagement of a crypto securities attorney before launch — this is a dependency, not an assumption
- Game engine for Atari uses open-source Gymnasium (formerly OpenAI Gym) environment wrappers
- Chess engine baseline uses python-chess with optional Stockfish integration for bot opponents
- A2A protocol integration is Day 1 via service card + REST wrapper; full autonomous A2A task delegation is Day 30

---

## Out of Scope (Phase 1)

- RTS and fighting game support (Phase 2)
- Mobile applications (web-first)
- XRP / XLM / Solana chain integrations (Phase 4 payment rails)
- Agent Identity NFT fractional revenue rights (Phase 2)
- US Accredited Pool vault (requires legal setup — Phase 2)
- Enterprise private arenas (Phase 3)
- DAO governance token (Phase 3)
- CodeFactor, CodeRabbit, or V12 C# source code — this spec is Arena Platform only

---

## Dependencies

- Crypto securities attorney engagement (required before US Accredited Pool launch)
- Smart contract audit firm engagement (required before any mainnet vault deployment)
- KYC/AML provider selection (Persona, Jumio, or equivalent) for Accredited Pool
- Stripe account with Connect for marketplace payment splitting
- Coinbase CDP or equivalent wallet infrastructure for on-chain AP2 payment mandates
- GCP project with billing enabled (existing VM is the starting point)
- Domain name and TLS certificate for public API endpoint

---

## Open Questions (Parked — Answer Before /iac.plan)

| # | Question | Impact |
|---|----------|--------|
| OQ-1 | Which EVM chain for launch? (Base / Polygon / BNB) | Affects smart contract deployment cost, tooling, and AP2 wallet SDK |
| OQ-2 | First game(s): Chess only, or Chess + Atari Breakout simultaneously? | Affects game engine infra complexity and launch timeline |
| OQ-3 | Does Agent Identity NFT include fractional revenue rights at launch? | Affects ERC-6551 vs plain ERC-721 decision for Phase 1 |
| OQ-4 | A2A full task delegation Day 1, or REST-only with A2A discovery endpoint? | Affects A2A protocol integration depth and testing surface |
| OQ-5 | Platform name | Affects domain, branding, and smart contract namespace |

---

## Notes

- This spec covers all six layers as a single infrastructure specification. Sub-specs per layer (`002-battle-arena`, `003-nft-registry`, etc.) will be generated after OQ-1 through OQ-5 are answered.
- The investment vault layer (Layer 6) is intentionally scoped to testnet / Open Pool only at launch. US Accredited Pool is Phase 2 pending legal setup.
- All revenue-bearing features that do NOT require financial licensing are in scope for Phase 1: entry fees, NFT minting, marketplace commissions, benchmark fees.
- Context Hub (`chub`) is installed at `/usr/local/bin/chub`. Run `chub get langgraph/package --lang py` before building the A2A connector layer.
- IaC Spec Kit Bob templates are at `/home/malhitticrypto/tools/iac-spec-kit/`.
