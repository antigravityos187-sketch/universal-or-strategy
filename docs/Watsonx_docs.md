IBM Building Blocks – Documentation¶
Building Blocks are pre-built, embeddable application capabilities that span AI (Agents, Trust, and Data) and Automation (Build, Observe, and Optimize). They are designed to accelerate innovation by enabling teams to rapidly infuse advanced IBM capabilities directly into their applications.

These ready-to-use components simplify the entire lifecycle—development, integration, deployment, and operation—allowing teams to deliver solutions faster and with significantly less complexity. Each Building Block acts as a reference implementation, demonstrating how IBM’s Data & AI and Automation platforms integrate seamlessly within real-world enterprise applications.

By adopting these proven and tested patterns, organizations can reduce engineering effort, minimize risk, and achieve a faster time-to-value while maintaining enterprise-grade scalability, security, and governance.

image

Capability Areas¶
AI Core Capabilities

Agents Reusable, enterprise-ready AI building blocks that accelerate agent adoption by enabling the design, orchestration, and deployment of intelligent agents across business workflows.

AI Trust AI Trust building blocks provide tools to evaluate AI models, test and monitor AI agents, enforce real-time safety guardrails, and manage regulatory compliance. They include model evaluation, agent ops, real-time guardrails, and AI compliance.

Data Core Capabilities

Integration: This is where data is brought together from different systems through pipelines, streaming, replication, unstructured ingestion, and observability. From a business perspective, this helps reduce silos and improves visibility across operations.

Intelligence: This is where the data becomes more useful and trusted. Quality checks, lineage, enrichment, and natural-language access help teams understand whether the data is accurate, explainable, and ready for decision-making.

Retrieval: This is the consumption layer. It enables users, applications, analytics, and AI solutions to access the right data through search, vector retrieval, NoSQL access, and federated analytics.

Automation Core Capabilities

Build Enables secure, automated integration and workflow orchestration across applications and clouds. These building blocks deliver identity and access control, infrastructure automation, and AI-assisted development to support faster, governed deployments.

Secure Provides end-to-end security across identities, applications, and infrastructure using IBM Verify and HashiCorp Vault for secure access and centralized secrets management. It ensures protection of credentials, tokens, and certificates across their lifecycle with strong governance and control. Advanced capabilities from IBM Quantum Safe enable quantum-resistant encryption and future-ready data protection across hybrid environments.

Optimize Delivers continuous monitoring, policy enforcement, and self-healing workflows. These building blocks enable cost-efficient operations, dynamic resource management, and real-time observability to maintain optimal performance, governance, and financial efficiency across hybrid and multicloud environments.

 
 
 AI Core - Building Blocks¶
Welcome to the AI Core Building Blocks documentation. This collection provides ready-to-use accelerators for building, deploying, and governing AI agents and applications with enterprise-grade trust and reliability.

Overview¶
This framework provides ready-to-use accelerators that address critical capabilities required to build, deploy, and govern AI agents and applications. These accelerators are designed to integrate seamlessly with existing enterprise systems, reducing time-to-value for AI projects.

AI Core Building Blocks Architecture

The AI Core building blocks provide a comprehensive framework organized into two core capabilities that work together to enable trustworthy, enterprise-grade AI:

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - AI

Architecture¶
The AI Core building blocks are organized into two core capabilities:

1. Agents¶
Build, orchestrate, and deploy intelligent AI agents

Agent building blocks accelerate the design, development, and deployment of intelligent agents across business workflows. From low-code agent builders to multi-agent orchestration and IDE-native development, these capabilities enable teams to rapidly create production-ready AI agents.

Key Capabilities: - Low-code agent development with IBM watsonx Orchestrate - Multi-agent coordination and workflow orchestration - IDE-native agentic SDLC with IBM Bob

2. AI Trust¶
Ensure reliability, safety, and compliance throughout the AI lifecycle

AI Trust building blocks provide comprehensive tools to evaluate models, monitor agents, enforce real-time guardrails, and maintain regulatory compliance. These capabilities ensure your AI solutions are trustworthy, transparent, and meet enterprise governance requirements.

Key Capabilities: - Model evaluation for quality, fairness, and bias - Agent operations monitoring and optimization - Real-time safety guardrails and compliance management

Agents Building Blocks¶
Agent capabilities focus on accelerating agent development and deployment.

Agent Builder¶
Build production-ready AI agents faster with low-code development tools powered by IBM watsonx Orchestrate. This accelerator provides pre-configured templates, skill libraries, and the Agent Development Kit (ADK) to streamline agent creation from concept to deployment.

Key Features:

Low-Code Development: Visual builder with drag-and-drop interface for rapid agent creation
Pre-Built Skills: Extensive library of ready-to-use skills and integrations
Agent Development Kit (ADK): Comprehensive SDK for custom agent development
Enterprise Integration: Connect to IBM and third-party systems seamlessly
Deployment Flexibility: Deploy agents across multiple channels and platforms
Use Cases: Customer service automation, IT operations, business process automation, knowledge management, workflow orchestration, data analysis

Multi-Agent Orchestration¶
Coordinate multiple specialized agents to solve complex business problems through intelligent orchestration. This accelerator enables agents to collaborate, share context, and execute coordinated workflows using industry-standard protocols like MCP and A2A.

Key Features:

Agent Coordination: Orchestrate multiple agents working together on complex tasks
Context Sharing: Enable agents to share information and maintain conversation state
Workflow Automation: Define and execute multi-step agent workflows
Protocol Standards: Support for MCP (Model Context Protocol) and A2A (Agent-to-Agent)
Scalable Architecture: Handle concurrent agent interactions with high reliability
Use Cases: Customer onboarding, complex approvals, multi-step troubleshooting, cross-functional workflows, intelligent routing, escalation management

Agentic SDLC¶
Transform software development with IBM Bob, an IDE-native agentic AI that automates the entire development lifecycle. From natural language requirements to production-ready code, Bob accelerates development while maintaining code quality and enterprise standards.

Key Features:

Intent-to-Software: Generate complete applications from natural language descriptions
IDE-Native: Embedded directly in VS Code for seamless developer experience
Development Modes: Specialized modes for coding, planning, debugging, and orchestration
Code Intelligence: Continuous awareness of entire codebase for accurate generation
Pipeline Integration: Extend AI assistance into terminals, CI/CD, and Git workflows
Use Cases: Greenfield development, legacy modernization, feature development, code quality improvement, documentation generation, CI/CD automation

AI Trust Building Blocks¶
AI Trust capabilities focus on ensuring reliability, safety, and compliance.

Model Evaluation¶
Evaluate AI and ML models for performance quality, fairness, reliability, drift, and bias before deployment. This accelerator provides comprehensive testing frameworks and metrics to ensure your models meet quality standards and business requirements.

Key Features:

Performance Metrics: Evaluate accuracy, precision, recall, and F1 scores
Fairness Testing: Detect and measure bias across demographic groups
Drift Detection: Monitor model performance degradation over time
Explainability: Understand model decisions and feature importance
Automated Testing: Continuous evaluation throughout the model lifecycle
Use Cases: Model validation, bias detection, performance monitoring, regulatory compliance, model comparison, quality assurance

Agent Ops¶
Monitor, evaluate, and optimize AI agents throughout their lifecycle with comprehensive observability and testing capabilities. This accelerator provides real-time insights into agent behavior, performance metrics, and quality indicators.

Key Features:

Real-Time Monitoring: Track agent performance, latency, and success rates
Quality Evaluation: Assess response quality, accuracy, and relevance
Behavior Analysis: Understand agent decision-making and interaction patterns
Performance Optimization: Identify bottlenecks and optimization opportunities
Testing Frameworks: Automated testing for agent capabilities and edge cases
Use Cases: Agent performance monitoring, quality assurance, behavior analysis, optimization, incident response, capacity planning

Real-Time Guardrails¶
Enforce safety boundaries and operational constraints to keep AI applications within desired behavior in production. This accelerator provides real-time content filtering, policy enforcement, and safety controls.

Key Features:

Content Filtering: Block harmful, inappropriate, or off-topic content
Policy Enforcement: Apply business rules and compliance policies in real-time
Safety Controls: Prevent hallucinations, toxic content, and security risks
Custom Rules: Define organization-specific guardrails and constraints
Low Latency: Minimal impact on response times with efficient filtering
Use Cases: Content moderation, compliance enforcement, brand protection, security controls, risk mitigation, user safety

AI Compliance¶
Ensure your AI applications meet regulatory requirements and industry standards for responsible AI use. This accelerator provides frameworks, documentation templates, and assessment tools for AI governance and compliance.

Key Features:

Regulatory Mapping: Map AI use cases to relevant regulations (GDPR, AI Act, etc.)
Risk Assessment: Evaluate AI systems for compliance and ethical risks
Documentation: Generate required documentation for audits and reviews
Governance Frameworks: Implement AI governance policies and controls
Audit Trails: Maintain comprehensive records of AI decisions and actions
Use Cases: Regulatory compliance, risk management, audit preparation, governance implementation, ethical AI, documentation management

Getting Started¶
Quick Start Guide

Follow these steps to get started with any building block:

Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks
Navigate to the specific building block directory (agents or ai-trust)

Follow the README instructions for setup and configuration

Key Benefits¶
Why Use AI Core Building Blocks?

Faster Development: Pre-built accelerators reduce agent development time
Enterprise Trust: Built-in governance, monitoring, and compliance
Production Ready: Battle-tested patterns and best practices
Flexibility: Modular design allows mix-and-match capabilities
Standards-Based: Support for industry protocols (MCP, A2A)
IBM Products Used¶
These building blocks leverage the following IBM products:

IBM watsonx Orchestrate: Agent development and orchestration platform
IBM watsonx.ai: Foundation models and AI services
IBM watsonx.governance: AI governance and compliance
IBM Bob: IDE-native agentic AI for software development
Contributing¶
We welcome contributions! Please fork the repository, create a feature branch, and open a pull request with your changes.

Contribution Guidelines

Follow existing code style and documentation patterns
Include tests for new features
Update documentation as needed
Ensure all tests pass before submitting
License¶
This project is licensed under the Apache 2.0 License.

Agents – AI Building Blocks¶
Enterprise-ready AI agents that automate business workflows, orchestrate complex tasks, and accelerate software development through intelligent automation. These building blocks provide the foundation for creating, deploying, and managing autonomous AI agents that integrate seamlessly with enterprise systems.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Agents Building Blocks

Overview¶
The Agents building blocks provide ready-to-use accelerators that make it easier to operationalize AI and GenAI use cases. Each accelerator addresses a critical capability required to build, integrate, and scale AI-driven applications. These accelerators are designed to integrate seamlessly with enterprise systems, reducing time-to-value for AI projects. By standardizing agent creation, orchestration, and governance, the framework ensures scalability, trust, and efficiency across diverse workloads.

Architecture¶
The Agents framework is organized into three core capabilities that work together to enable intelligent automation:

Agents Building Blocks Architecture

The architecture demonstrates how these building blocks integrate:

Agentic SDLC¶
Transform software development with IBM Bob, an IDE-native agentic AI that automates the entire development lifecycle from natural language requirements to production-ready code.

Watsonx Orchestrate¶
Build and deploy production-ready AI agents with low-code development tools, pre-built skills, and enterprise integrations powered by IBM watsonx Orchestrate.

Core Building Blocks¶
Agent Builder - Create AI agents rapidly using visual builder, Agent Development Kit (ADK), pre-built skills library, and natural language to agent conversion
Multi-Agent Orchestration - Coordinate multiple specialized agents through A2A and MCP protocols to solve complex business problems with intelligent workflow automation
These components work together seamlessly: Agent Builder creates individual agents, Multi-Agent Orchestration coordinates them for complex workflows, all within the watsonx Orchestrate platform, and supported by IBM Bob's Agentic SDLC for development automation.

Building Blocks¶
Agent Builder¶
Create and deploy autonomous AI agents that interact with enterprise applications, tools, and data using the watsonx Orchestrate Agentic Development Kit (ADK).

Key Features:

Python-based ADK - Build agents using Python library and CLI tools
Tool Integration - Connect agents to enterprise systems and APIs
Prompt Configuration - Define agent instructions, rules, and boundaries
Lifecycle Management - Version, test, and deploy agents with confidence
Developer Edition - Local development and rapid iteration
Use Cases:

Customer service automation
HR process automation
IT support and troubleshooting
Data analysis and reporting
Workflow orchestration
Multi-Agent Orchestration¶
Coordinate multiple AI agents to collaborate intelligently on complex enterprise workflows, with support for external system integration through MCP and A2A protocols.

Key Features:

Dynamic Task Delegation - Automatically route tasks to specialized agents
Shared Memory & Context - Agents exchange knowledge through unified memory
Chained Reasoning - Combine outputs from multiple agents
MCP Integration - Connect to external systems via Model Context Protocol
A2A Protocol - Enable agent-to-agent communication across platforms
Use Cases:

Cross-functional business processes
Complex decision-making workflows
Enterprise data access and synchronization
Distributed agent systems
Third-party agent integration
Agentic SDLC¶
Transform software development with IBM Bob, an IDE-native agentic AI that automates the entire software development lifecycle from intent to production-ready code.

Key Features:

Intent-to-Software Generation - Natural language to production code
Agentic Development Modes - Specialized modes for different tasks
In-Context Code Intelligence - Continuous codebase awareness
Real-Time Code Review - Automated refactoring and quality checks
Pipeline Integration - CI/CD automation and testing
Use Cases:

Greenfield application development
Legacy code modernization
Feature development and enhancement
Documentation generation
CI/CD workflow automation
Key Capabilities¶
Agent Development¶
Low-Code Agent Creation - Build agents with minimal coding using ADK
Tool & API Integration - Connect agents to enterprise systems
Prompt Engineering - Configure agent behavior and instructions
Testing & Validation - Evaluate agent performance before deployment
Version Control - Manage agent configurations as code
Agent Orchestration¶
Multi-Agent Coordination - Orchestrate multiple specialized agents
Context Management - Share knowledge across agent interactions
Task Routing - Intelligently delegate tasks to appropriate agents
External Integration - Connect to systems via MCP and A2A protocols
Human-in-the-Loop - Escalate to humans when needed
Development Automation¶
Code Generation - Transform requirements into production code
Refactoring - Automated code improvement and modernization
Code Review - AI-assisted quality and security analysis
Documentation - Automatic documentation generation
CI/CD Integration - Automated testing and deployment
Getting Started¶
Choose the building block that matches your current need.
Explore the assets folder in the repository for ready-to-use code samples and SDKs.
Check bob-modes for AI-assisted agent development workflows.
GitHub Repository

Agents Building Blocks

Agent Builder¶
Create and deploy autonomous AI agents that interact with enterprise applications, tools, and data using the watsonx Orchestrate Agentic Development Kit (ADK).

Why This Matters¶
Agent-based automation is complex. Building production-ready agents requires orchestrating LLMs, tools, memory, and enterprise integrations—without a framework, teams spend months on infrastructure instead of business logic.
Manual agent development doesn't scale. Hand-coding every agent interaction, tool call, and error path creates brittle systems that break when requirements change.
Enterprise agents need governance. Agents accessing sensitive data and systems require versioning, testing, observability, and audit trails that ad-hoc implementations can't provide.
Deployment friction slows adoption. Moving agents from development to production involves complex infrastructure, authentication, and monitoring setup that delays time-to-value.
Documentation access accelerates development. Agent Builder comes with a pre-configured ADK documentation MCP server, providing instant access to API references, examples, and best practices directly within your development workflow.
What's Covered¶
Component	What It Provides
Agent Development Kit (ADK)	Python library and CLI for building agents on watsonx Orchestrate
Core Capabilities	Prompt configuration, tool integration, evaluation, and lifecycle management
Agent Development Kit (ADK)¶
Build agents using a Python-based framework that runs on the watsonx Orchestrate platform.

Key Features¶
Feature	Description
Python Library	Programmatic agent creation with Python SDK
CLI Tool	Command-line interface for agent management and deployment
watsonx Orchestrate Integration	Native deployment to enterprise orchestration platform
Framework Interoperability	Integrate agents and tools built on other frameworks (LangChain, CrewAI, etc.)
Core Capabilities¶
Prompt Configuration¶
Define agent behavior through natural language instructions:

System Instructions - Core agent purpose and behavior guidelines
Conversation Rules - How agents should interact with users
Boundaries & Constraints - What agents should and shouldn't do
Escalation Paths - When to involve human oversight
Tool Integration¶
Connect agents to enterprise systems and APIs:

Pre-built Connectors - Ready-to-use integrations for common systems
Custom Tools - Build custom tool integrations for proprietary systems
API Wrappers - Automatically generate tools from OpenAPI specifications
Database Access - Query databases directly from agents
Agent Evaluation¶
Test and validate agent performance before deployment:

Accuracy Testing - Verify agents produce correct outputs
Behavior Validation - Ensure agents follow instructions and boundaries
Performance Metrics - Measure response time and resource usage
Safety Checks - Test for harmful outputs and security issues
Lifecycle Management¶
Manage agents throughout their operational lifecycle:

Versioning - Track agent changes over time
Testing - Automated testing before deployment
Deployment - Push agents to production environments
Monitoring - Track agent performance and usage
Updates - Roll out improvements and fixes
Use Cases¶
Customer Service - Handle inquiries, route tickets, and provide 24/7 automated support
HR Automation - Automate leave approvals, benefits enrollment, and policy queries using Workday APIs
Finance Operations - Process expense reports, invoices, and compliance checks
Data Analysis - Query databases, generate reports, and provide insights
IT Support - Troubleshoot issues, monitor systems, and automate incident management
Content Generation - Create documentation, summaries, and communications
Resources¶
ADK Documentation - Complete API reference, guides, and examples for the Agentic Development Kit
Bob Mode for Agent Builder - AI-assisted workflow for creating and deploying agents
GitHub Repository

Agent Builder Assets

Multi-Agent Orchestration¶
Coordinate multiple AI agents to collaborate intelligently on complex enterprise workflows, with support for external system integration through MCP and A2A protocols.

Why This Matters¶
Single agents hit complexity limits. Complex business processes span multiple domains—HR, IT, Finance, Legal—and no single agent can master all of them effectively.
Manual coordination doesn't scale. Hand-coding agent interactions, context passing, and error handling creates brittle workflows that break when requirements change.
Enterprise systems are distributed. Business data and logic live across Salesforce, SAP, Workday, and custom applications—agents need seamless access to all of them.
Cross-platform collaboration is hard. Agents built on different frameworks (LangChain, CrewAI, custom) can't easily work together without standardized protocols.
Governance requires orchestration. Multi-agent systems need centralized coordination, monitoring, and audit trails to maintain control and compliance.
What's Covered¶
Component	What It Provides
Orchestration Flow	Example showing customer verification and onboarding workflow
Key Capabilities	Dynamic task delegation, shared memory, chained reasoning, and external integration
Integration Standards	MCP and A2A protocols for external system and agent collaboration
Orchestration Flow¶
Multi-agent orchestration enables seamless collaboration across specialized agents to handle complex workflows.

Multi-Agent Orchestration Example

Example: Customer Verification & Onboarding

This workflow demonstrates how multiple agents collaborate to onboard a new client:

Interaction - Customer Success Manager initiates request via Chat, Slack, or Voice
Orchestration - Customer verification agent coordinates the workflow
Collaboration - Specialized agents work together:
Company profiling agent - Gathers business information
Agentforce onboarding agent - Handles Salesforce integration
Legal advisor agent - Reviews compliance requirements
Tool Use - Agents leverage various tools and systems:
APIs & Apps (Salesforce, SAP Ariba, Dun & Bradstreet, Milvus)
Actions and dialog flows
Knowledge bases and documents
Business automation flows
Data sources (SQL, etc.)
Built-in tools (web search, etc.)
Key Capabilities¶
Dynamic Task Delegation¶
Automatically route tasks to the most capable agent or external system:

Intelligent Routing - Analyze task requirements and agent capabilities
Load Balancing - Distribute work across available agents
MCP/A2A Integration - Delegate to external systems when needed
Fallback Handling - Reroute tasks when agents are unavailable
Shared Memory & Context¶
Enable agents to exchange knowledge through unified memory:

Context Preservation - Maintain conversation history across agent handoffs
Structured Knowledge - Share data in standardized formats
Memory Layers - Short-term (conversation) and long-term (knowledge base) memory
Cross-Agent Access - All agents can read and write to shared context
Chained Reasoning¶
Combine outputs from multiple agents for comprehensive responses:

Sequential Processing - Pass results from one agent to the next
Parallel Execution - Run multiple agents simultaneously
Result Synthesis - Combine insights from different agents
Confidence Scoring - Aggregate confidence levels across agents
Goal-Driven Execution¶
Orchestrate end-to-end workflows from intent to completion:

Intent Detection - Understand user goals and requirements
Task Decomposition - Break complex goals into subtasks
Progress Tracking - Monitor workflow execution status
Error Recovery - Handle failures and retry logic
External System Integration¶
MCP Servers - Connect to external systems and data sources:

Secure gateways for enterprise systems
Protocol mediation and data transformation
Event bridging for real-time updates
Controlled access to sensitive data
A2A Servers - Enable agent-to-agent collaboration:

Workflow extension across platforms
Transaction orchestration
Scalable integration patterns
Third-party agent discovery and binding
Integration Standards¶
MCP (Model Context Protocol)¶
Open standard for secure connections between data sources and AI tools:

Two-Way Communication - Bidirectional data flow between agents and systems
Server Deployment - Expose enterprise data through MCP servers
Client Integration - Build agents that connect to MCP servers
Security - Authentication, authorization, and encryption
A2A (Agent-to-Agent)¶
Open standard for cross-platform agent collaboration:

Agent Discovery - Find and connect to agents regardless of platform
Protocol Standardization - Common communication format
Framework Agnostic - Works with LangChain, CrewAI, custom agents
Interoperability - Seamless collaboration across technologies
Core Principles¶
Interoperability¶
Enable agents built on any framework to communicate and collaborate
Support different agent styles (Default, ReAct, Planner) for task resolution
Standardization¶
Provide consistent interfaces and protocols using MCP and A2A standards
Ensure predictable behavior across agent interactions
Extensibility¶
Allow for future expansion as agent technologies evolve
Support new agent types and integration patterns
Simplicity¶
Make integration straightforward for business users and developers
Minimize configuration and setup complexity
Security¶
Ensure secure communication and data handling between agents
Implement authentication, authorization, and audit logging
Use Cases¶
Employee Onboarding - HR agent coordinates with IT and Finance agents for complete onboarding workflow
Customer Verification - Verification agent collaborates with profiling, onboarding, and legal agents
Complex Decision-Making - Multiple specialized agents analyze different aspects of business problems
Enterprise Data Access - Agents connect to databases, knowledge bases, and document repositories via MCP
Cross-Platform Collaboration - Agents built on different frameworks work together through A2A protocol
Third-Party Integration - External AI agents join orchestrated workflows seamlessly
Resources¶
Bob Mode for Multi-Agent Orchestration - AI-assisted workflow for building multi-agent systems
GitHub Repository

Multi-Agent Orchestration Assets

Agentic SDLC¶
Automate the entire software development lifecycle with IBM Bob, an IDE-native agentic AI that transforms natural language requirements into production-grade code, services, and complete projects.

Why This Matters¶
Manual coding is slow and repetitive. Developers spend significant time on boilerplate, documentation, and routine refactoring instead of solving business problems.
Context switching breaks flow. Moving between IDE, documentation, Stack Overflow, and code reviews fragments developer attention and reduces productivity.
Code quality degrades over time. Without continuous refactoring and review, technical debt accumulates and maintenance becomes increasingly difficult.
Enterprise standards are hard to enforce. Ensuring code follows organizational patterns, security practices, and style guides requires constant vigilance.
CI/CD pipelines need manual intervention. Build failures, test issues, and deployment problems require developers to context-switch and troubleshoot.
What's Covered¶
Component	What It Provides
IBM Bob	IDE-native agentic AI for software development automation
Key Features & Capabilities	Intent-to-software generation, development modes, code intelligence, and CI/CD integration
Powered by IBM Bob¶
IBM Bob Logo

Agent Harness is powered by IBM Bob, an IDE-native agentic AI that serves as your SDLC partner, embedded directly in developer workflows with deep, continuous awareness of real codebases.

What Makes Bob Different:

IDE-Native - Works directly in VS Code, not as a separate tool
Context-Aware - Maintains continuous understanding of your entire codebase
Mode-Based - Specialized modes for different development tasks
Pipeline-Integrated - Extends beyond IDE into terminals and CI/CD
Enterprise-Ready - Supports organizational standards and governance
Key Features & Capabilities¶
Intent-to-Software Generation¶
Transform natural language requirements into production-ready code:

Natural Language Input - Describe what you want in plain English
Context-Aware Generation - Code aligns with repository structure and dependencies
Complete Projects - Generate entire applications, not just snippets
Incremental Development - Extend existing systems with new features
Multiple Languages - Support for Python, JavaScript, Java, Go, and more
Agentic Development Modes¶
Purpose-built modes define Bob's behavior for different development tasks:

Mode	Purpose	Use When
Code Mode	Create new code from requirements	Building new features or applications
Ask Mode	Refactor or enhance existing code	Improving code quality or adding functionality
Plan Mode	Design and strategize before implementation	Planning architecture or complex changes
Advanced Mode	Access to MCP and Browser tools	Complex integrations or web interactions
Orchestrator Mode	Coordinate multi-step workflows	Managing complex, multi-phase development tasks
Modes enable efficient development without changing how developers communicate.

In-Context Code Intelligence¶
Maintain continuous awareness of your codebase:

Full Repository Understanding - Bob knows your entire project structure
Dependency Tracking - Understands relationships between files and modules
Safe Refactoring - Changes consider downstream impacts
Accurate Generation - New code follows existing patterns and conventions
Error Prevention - Reduces bugs from incomplete context
Real-Time Code Review & Refactoring¶
Automated code quality and improvement:

Inline Analysis - Detect complexity issues, code smells, and anti-patterns
Refactoring Suggestions - Actionable recommendations to reduce technical debt
Security Scanning - Identify potential security vulnerabilities
Performance Optimization - Suggest improvements for better performance
Style Enforcement - Ensure code follows organizational standards
Pipeline-Wide Integration¶
Extend AI assistance beyond the IDE:

Terminal Integration - AI assistance in command-line workflows
CI/CD Automation - Automated build, test, and release support
Git Integration - Intelligent commit messages and PR descriptions
Test Generation - Automatic test creation and maintenance
Documentation - Generate and update documentation automatically
Use Cases¶
Greenfield Development - Generate complete applications from requirements and architecture descriptions
Legacy Modernization - Refactor and restructure legacy code with automated documentation updates
Feature Development - Implement new features faster with fewer regressions
Code Quality - Automated refactoring and debugging to reduce manual effort
Documentation - Generate documentation from existing codebases for better maintainability
CI/CD Automation - Automate repetitive engineering tasks and boilerplate code
Resources¶
Bob Website - Learn more about IBM Bob and its capabilities
Download Bob - Get started with IBM Bob for VS Code
GitHub Repository

Agentic SDLC Assets

AI Trust¶
Building trust in AI requires a holistic approach across the full AI lifecycle — from model evaluation and agent operations to real-time safeguards and regulatory compliance. These capabilities are powered by IBM watsonx governance and IBM watsonx orchestrate.

The AI Trust building blocks provide frameworks, production-ready code samples, and tools to help you build AI solutions that are reliable, transparent, and compliant. Whether you're evaluating GenAI models for quality and safety, testing AI agents before deployment, enforcing real-time guardrails in production, or mapping AI use cases to regulations — AI Trust has you covered.

AI Trust Building Blocks

Building Blocks¶
Building Block	What It Does
Model Evaluation	Evaluate your AI and ML models for performance quality, fairness, reliability, drift, and bias
Agent Ops	Evaluate, observe, and optimize your AI agents throughout the lifecycle
Real-Time Guardrails	Enforce safety boundaries and operational constraints to keep AI applications within desired behavior in production
AI Compliance	Ensure your AI applications meet regulatory requirements and industry standards for responsible AI use
Getting Started¶
Choose the building block that matches your current need.
Explore the assets folder in the repository for ready-to-use code samples and SDKs.
Check bob-modes for AI-assisted evaluation workflows.
GitHub Repository

AI Trust Building Blocks

Model Evaluation¶
Evaluate your AI and ML models for a range of key metrics — performance quality, fairness, reliability, drift, bias, and more — throughout the AI lifecycle.

Why This Matters¶
Unvalidated systems fail at the edges. LLM pipelines can hallucinate, leak sensitive data, or degrade when upstream data changes. Evaluation surfaces these issues before release.
Production failures are costly. Issues like PII leakage or ungrounded responses become significantly harder to diagnose once embedded in live workflows.
Compliance requires evidence. Regulatory frameworks such as the EU AI Act and NIST AI RMF expect structured testing with reproducible scoring and stored evaluation artifacts.
Baselines enable monitoring. Metrics captured at evaluation time become reference points for detecting drift and regression in production.
What's Covered¶
Area	What It Evaluates
Gen AI Evaluations	RAG pipelines, LLM outputs, chatbot safety — quality, safety, readability metrics
Predictive ML Evaluations	Traditional ML models — scoring, confidence assessment, credit risk prediction
Gen AI Evaluations¶
Evaluate generative AI applications — RAG pipelines, LLM outputs, and chatbot safety — using IBM watsonx governance metrics.

Evaluation Scripts¶
Script	What It Evaluates
RAG Quality	Answer relevance, faithfulness, context relevance, retrieval precision, NDCG
Content Safety	HAP, PII, jailbreak, social bias, violence, profanity (15 metrics)
LLM-as-Judge	Evasiveness detection, topic relevance with system prompt boundaries
Readability	Text grade level, Flesch reading ease
Deployment Readiness	Combined quality + safety check with pass/fail verdict
Metrics Reference¶
Metric	Category	Description
Faithfulness	Quality	Is the response grounded in the provided context?
Answer Relevance	Quality	Does the response address the user's question?
Answer Similarity	Quality	Semantic similarity to a ground-truth reference
Context Relevance	Retrieval	Are retrieved passages relevant to the query?
Retrieval Precision	Retrieval	Proportion of retrieved passages that are relevant
NDCG	Retrieval	Ranking quality of retrieved results
Hit Rate	Retrieval	Did at least one relevant passage get retrieved?
HAP	Safety	Hate, abuse, and profanity detection
PII	Safety	Personally identifiable information detection
Jailbreak	Safety	Prompt injection / jailbreak attempt detection
Social Bias	Safety	Stereotyping and discriminatory language
Evasiveness	Quality	Is the model dodging the question?
Topic Relevance	Quality	Is the response on-topic?
Text Grade Level	Readability	US school grade needed to understand the text
Text Reading Ease	Readability	Flesch Reading Ease score (0–100)
Predictive ML Evaluations¶
Evaluate predictive ML models deployed on IBM watsonx ML — scoring, confidence assessment, and interactive exploration.

Available Assets¶
Asset	What It Does
Credit Risk Prediction App	Interactive Dash web app for credit risk scoring with real-time predictions
Model Scoring API	Direct REST API calls to deployed watsonx ML models — suitable for batch scoring and pipeline integration
Both assets authenticate via IBM Cloud IAM and call deployed watsonx ML model endpoints.

Bob Modes¶
A Bob mode for Gen AI evaluation is available, providing an AI-assisted workflow that guides you through the evaluation process step by step.

GitHub Repository

Model Evaluation Assets

Agent Ops¶
AI agents don't behave like traditional software — they can respond differently every time. That makes them harder to test, trust, and troubleshoot. Agent Ops is a framework for testing, monitoring, and improving AI agents from development through production.

The capabilities below are built for watsonx Orchestrate agents using the Agent Development Kit (ADK). For LangGraph/LangChain agents, see LangGraph Agent Evaluation at the bottom of this page.

Evaluate, observe, and optimize your agents using the Agent Ops Building Block

Why This Matters¶
Agents are non-deterministic. The same input can produce different outputs, making traditional testing insufficient.
Failures are hard to diagnose. When an agent calls the wrong tool or hallucinates a response, tracing the root cause requires structured analysis.
Manual testing doesn't scale. Testing every user scenario by hand creates bottlenecks that slow deployment.
Cost and latency are unpredictable. Without observability, agents can burn through token budgets or create unacceptable latency without anyone noticing.
Capabilities¶
Capability	What It Does
Evaluate	Simulate real users at scale to verify the agent does what it's supposed to do
Analyze	Pinpoint exactly where and why an agent went wrong
Quick-Eval	Fast sanity check — catch structural issues early without writing full test cases
Generate	Turn plain-English user stories into automated test scenarios
Red-Team	Stress-test agent security against prompt injection, social engineering, and jailbreaking
Observe	Track cost, latency, and token usage per interaction with full traceability
Evaluation Workflow¶
Quick-Eval
Sanity check
Generate
Create benchmarks
Evaluate
Full testing
Analyze
Diagnose failures
Red-Team
Security testing
Observe
Cost & latency
Quick-Eval — Fast referenceless validation to catch tool schema issues
Generate — Auto-create benchmarks from plain-English user stories
Evaluate — Run full evaluation with LLM-simulated users
Analyze — Diagnose failures with default and enhanced analysis modes
Red-Team — Test against 15 adversarial attack types
Observe — Track cost, latency, and token usage via Langfuse
Metrics Reference¶
Agent Metrics¶
Metric	Target	What It Measures
Journey Success	1.0	All goals completed (binary)
Journey Completion %	100%	Percentage of goals met
Tool Call Precision	>= 0.5	Correct calls / total calls made
Tool Call Recall	>= 0.9	Expected calls made / total expected
Agent Routing F1	>= 0.9	Harmonic mean of precision and recall
RAG Metrics¶
Metric	Target	What It Measures
Faithfulness	>= 0.8	Answer grounded in retrieved docs
Answer Relevancy	>= 0.7	Answer addresses the question
Response Confidence	> 0.5	LLM confidence in generated response
Red-Teaming Attack Types¶
Category	Attacks
On-policy	instruction_override, emotional_appeal, role_playing, hypothetical_scenario, authority_impersonation, crescendo_attack
Off-policy	jailbreaking, prompt_leakage, topic_derailment, social_engineering, data_extraction
LangGraph Agent Evaluation¶
For teams building agents with LangGraph or LangChain, a Python SDK package (wx_gov_agent_eval) is also available. It provides three evaluator classes — BasicRAG, ToolCalling, and AdvancedRAG — integrated with IBM watsonx governance for metrics and factsheet tracking.

LangGraph Agent Evaluation Assets

Bob Modes¶
A Bob mode for Agent Ops evaluation is available, providing an AI-assisted workflow for automated agent evaluation with WXO agents.

GitHub Repository

Agent Ops Assets

Real-Time Guardrails¶
Enforce safety boundaries and operational constraints to keep your AI applications within desired behavior in production. Guardrails evaluate every AI input and output against configurable thresholds — blocking, flagging, or passing content before it reaches users.

Why This Matters¶
Production failures are visible and costly. When a model generates harmful content, leaks PII, or returns irrelevant answers in front of customers, the impact is immediate — regulatory exposure, reputational damage, and loss of trust.
Safety can't be solved at design time alone. Users will find ways to misuse AI systems that pre-deployment testing cannot anticipate. Real-time guardrails provide the last line of defense.
Compliance requires ongoing protection. Regulatory frameworks expect organizations to demonstrate continuous safeguards, not just pre-deployment testing.
How It Works¶
block
pass
pass
flag
block
User Input
Input Guardrail
Jailbreak, HAP
Input rejected
AI Model
Output Guardrail
PII, Safety, Quality
PASS / FLAG / BLOCK
Response delivered
Delivered + flagged
Fallback response
Three-tier response handling:

PASS — Content is within acceptable limits, serve normally
FLAG — Content is borderline, serve but log for human review
BLOCK — Content violates thresholds, serve a fallback response instead
Guardrail Types¶
Built-in Content Safety¶
Detects harmful content using IBM watsonx governance pre-trained models. No additional setup beyond API credentials.

Metric	What It Detects	Threshold Type
HAP	Hate, abuse, and profanity	Upper-limit (block when exceeded)
PII	Names, emails, SSNs, phone numbers	Upper-limit
Jailbreak	Prompt injection and jailbreak attempts	Upper-limit
Social Bias	Stereotyping and discriminatory language	Upper-limit
Violence	Violent content	Upper-limit
Profanity	Profanity	Upper-limit
Harm	General harm	Upper-limit
Sexual Content	Sexual content	Upper-limit
Unethical Behavior	Unethical content	Upper-limit
Evasiveness	Evasive or non-committal responses	Upper-limit
RAG Quality Guardrails¶
Real-time quality checks on RAG pipeline responses. Blocks responses that are hallucinated or off-topic.

Metric	What It Checks	Threshold Type
Answer Relevance	Does the response address the question?	Lower-limit (block when quality drops)
Context Relevance	Are retrieved passages relevant?	Lower-limit
Faithfulness	Is the response grounded in context?	Lower-limit
Custom LLM-as-Judge Guardrails¶
Define your own guardrail criteria using an LLM evaluator. Two approaches:

Prompt template — Full control over the evaluation prompt (e.g., answer completeness)
Criteria + Options — Structured rubric with named options and scores (e.g., conciseness, helpfulness)
Uses LLMAsJudgeMetric with WxAIFoundationModel as the judge.

Available Assets¶
Script	What It Does
Content Safety Guardrails	Screen inputs/outputs for 10 safety metrics with configurable BLOCK/FLAG/PASS thresholds
RAG Quality Guardrails	Real-time faithfulness, relevance, context quality checks with fallback responses
Custom Guardrails	Define custom LLM-as-judge guardrails (completeness, conciseness, helpfulness)
Guardrail Pipeline	End-to-end: validate input → call model → validate output → audit log
All scripts use the ibm_watsonx_gov SDK with MetricsEvaluator and GenAIConfiguration.

GitHub Repository

Real-Time Guardrails Assets

AI Compliance¶
AI regulations are multiplying fast and every AI use case may fall under different rules. Without a systematic approach, compliance becomes a bottleneck to deploying AI — or a risk if missed entirely.

Why This Matters¶
Regulatory pressure is growing. The EU AI Act, NIST AI RMF, ISO 42001, and other frameworks impose concrete requirements on AI transparency, risk assessment, and human oversight.
Manual compliance is unsustainable. Assembling evaluation results, monitoring logs, and risk documentation by hand for every model doesn't scale.
Hidden compliance gaps create risk. Without a centralized view of which regulations apply to which AI use cases, gaps go undetected until an audit.
Compliance is continuous. Models change, data drifts, and regulations evolve. Organizations need a governance posture that stays current automatically.
Key Capabilities¶
Capability	What It Does
Map AI use cases to regulations	Compliance plans identify which rules apply by use case and region
Position reporting	Surface potential compliance gaps across the enterprise
Configurable assessment workflows	Streamline the review cycle for use case owners and compliance teams
Available Assets¶
Script	What It Does
Use Case Inventory Management	Create and manage AI use cases in the watsonx governance inventory, add compliance metadata (risk level, regulations, ownership)
Governed Tool Catalog	Register, list, and manage AI tools in the watsonx governance tool catalog
Compliance Workflows (OpenPages Governance Console)¶
For full compliance lifecycle management — regulation mapping, risk assessment, and position reporting — use the IBM OpenPages Governance Console integrated with watsonx governance.

Workflow	What It Does
Regulatory Compliance Management	Map AI use cases to regulations (EU AI Act, NIST AI RMF), track regulatory changes
Risk Identification & Assessment	Run risk assessments with configurable questionnaires
Position Reporting	Dashboard-based visibility into compliance posture across the enterprise
AI Risk Atlas	Built-in guide to AI risks for planning risk mitigation
Setting Up OpenPages Integration¶
Provision an OpenPages instance with "Model Risk Governance" solution
Integrate with watsonx governance (API key + fixed URL)
Load solution files (questionnaire templates, risk atlas content, sample AI mandates)
Create AI use cases in the Governance Console to access compliance workflows
Learn More

Integrating watsonx governance with OpenPages
Managing risk with Governance Console
Creating use cases in Governance Console
IBM AI Governance Facts Client samples
GitHub Repository

AI Compliance Assets

Data - Building Blocks¶
Welcome to the Data Building Blocks documentation. This collection provides ready-to-use accelerators organized into three main categories: Integration, Intelligence, and Retrieval.

Overview¶
This framework provides ready-to-use accelerators that address critical capabilities required to manage, process, and secure data for AI-driven applications. These accelerators are designed to integrate seamlessly with existing enterprise systems, reducing time-to-value for AI projects.

Data Building Blocks Overview

The Data building blocks provide a comprehensive data management framework organized into three core capabilities that work together to enable AI-driven applications:

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Data

Architecture¶
The Data building blocks are organized into three core capabilities that form a complete data lifecycle:

1. Integration¶
Bring data into your systems efficiently and reliably

Data ingestion and pipeline automation capabilities that connect to various data sources, transform data, and load it into your data platform. Includes AI-powered pipeline generation, real-time streaming, and comprehensive observability.

Key Capabilities: - AI-generated data pipelines for rapid development - Real-time event streaming with Confluent - Pipeline monitoring and data quality validation

2. Intelligence¶
Ensure data quality, governance, and traceability

Data quality, governance, and lineage tracking capabilities that ensure your data is trustworthy, compliant, and traceable throughout its lifecycle. Includes automated quality checks, end-to-end lineage tracking, and natural language query generation.

Key Capabilities: - Automated data quality validation and monitoring - Complete data lineage tracking for compliance - Natural language to SQL query conversion

3. Retrieval¶
Access and query data for AI applications

Data access and retrieval capabilities that enable AI applications to efficiently query and retrieve data. Includes vector search for semantic similarity, NoSQL storage for scalability, and zero-copy federated analytics.

Key Capabilities: - Vector search for RAG and semantic retrieval - Scalable NoSQL database with Cassandra compatibility - Federated analytics without data duplication

Integration Building Blocks¶
Integration capabilities focus on data ingestion and pipeline automation.

Data Pipeline (AI Generated)¶
Transform how you build data pipelines with AI-powered generation and automation. This accelerator uses IBM watsonx.ai to automatically generate optimized data pipelines for both structured and unstructured data sources, dramatically reducing development time from weeks to hours.

Key Features:

AI-Powered Generation: Automatically generate complete data pipelines using natural language descriptions
Unstructured Data Support: Process documents, PDFs, images, and media files with built-in extraction
Structured Data Integration: Connect to RDBMS sources with Change Data Capture (CDC) support
Flexible Ingestion Modes: Support for both batch and real-time streaming ingestion
watsonx.data Integration: Seamless integration with IBM's open lakehouse platform
Use Cases: Document processing, database migration, real-time data synchronization, data lake population

Data Streaming¶
Enable real-time data processing with enterprise-grade streaming capabilities powered by Confluent Platform. Capture, process, and route data streams in real-time to power AI applications, analytics, and operational systems with low-latency data delivery.

Key Features:

Real-Time Event Ingestion: Capture and process millions of events per second with Confluent Platform
Advanced Stream Processing: Transform data in-flight using ksqlDB, Kafka Streams, and Apache Flink
200+ Pre-Built Connectors: Integrate with databases, cloud services, and applications via Kafka Connect
Schema Registry: Centralized schema management for data governance and compatibility
Stream Governance: Built-in data quality, lineage, and security controls
Use Cases: Real-time analytics, event-driven architectures, microservices integration, IoT data processing

Data Observability¶
Gain complete visibility into your data pipelines with comprehensive monitoring, alerting, and quality validation. Powered by Databand, this accelerator helps teams detect, diagnose, and resolve data quality issues before they impact downstream applications and AI models.

Key Features:

Pipeline Monitoring: Real-time tracking of pipeline execution, performance metrics, and bottleneck identification
Data Quality Validation: Automated quality checks, schema validation, and anomaly detection
Intelligent Alerting: Configurable alerts with multi-channel notifications (email, Slack, PagerDuty)
Historical Analysis: Trend analysis and SLA monitoring for continuous improvement
Native Integration: Seamless integration with IBM watsonx.data and popular orchestration tools
Use Cases: Pipeline health monitoring, data quality assurance, incident response, compliance reporting

Intelligence Building Blocks¶
Intelligence capabilities focus on data quality, governance, and lineage tracking.

Data Quality¶
Maintain trustworthy data for AI applications with automated quality validation and monitoring. This accelerator provides comprehensive data quality checks, profiling, and validation rules to ensure your data meets business requirements and quality standards.

Key Features:

Automated Validation: Define and enforce data quality rules across your data estate
Quality Monitoring: Continuous assessment of data quality metrics and trends
Data Profiling: Automated profiling to understand data characteristics and patterns
Anomaly Detection: Identify data quality issues and anomalies in real-time
watsonx.data Intelligence: Native integration for enterprise-grade data governance
Use Cases: Data quality assurance, regulatory compliance, AI model accuracy, data cleansing

Data Lineage¶
Achieve complete visibility into data flow and transformations across your organization. Track data from source to destination, understand dependencies, and assess the impact of changes with automated lineage capture and visualization.

Key Features:

End-to-End Tracking: Automatic lineage capture from data pipelines and transformations
Column-Level Lineage: Track individual column transformations and dependencies
Impact Analysis: Assess downstream effects of schema changes and data modifications
Compliance Support: Generate audit trails and lineage reports for regulatory requirements
Visual Lineage Maps: Interactive visualization of data flows and relationships
Use Cases: Regulatory compliance (GDPR, CCPA), impact analysis, root cause analysis, migration planning

Text2SQL¶
Democratize data access by enabling users to query databases using natural language instead of SQL. Powered by IBM watsonx.ai foundation models, this accelerator translates natural language questions into optimized SQL queries, making data accessible to non-technical users.

Key Features:

Natural Language Understanding: Interpret complex questions with context awareness
Intelligent SQL Generation: Generate syntactically correct, optimized SQL queries
Schema Intelligence: Automatic understanding of table relationships and business terms
Multi-Database Support: Compatible with PostgreSQL, MySQL, Db2, and other databases
Query Validation: Built-in syntax validation and security checks
Use Cases: Business intelligence, ad-hoc analysis, self-service analytics, report generation

Retrieval Building Blocks¶
Retrieval capabilities enable AI applications to access and query data efficiently.

Vector Search¶
Build powerful RAG (Retrieval-Augmented Generation) systems with high-performance vector search capabilities. This accelerator provides document ingestion, embedding generation, and semantic similarity search to enable AI applications to retrieve relevant information based on meaning, not just keywords.

Key Features:

Document Processing: Automated parsing and extraction from multiple file formats
Flexible Embedding: Support for dense, hybrid, and dual embedding strategies
Semantic Search: Find documents based on meaning and context
REST API: Production-ready API with authentication and rate limiting
Multiple Backends: Support for Milvus, OpenSearch, and DataStax Astra DB
Supported Databases:

Milvus: High-performance open-source vector database
OpenSearch: Hybrid vector and keyword search capabilities
DataStax Astra DB: Cloud-native serverless vector database
Use Cases: RAG systems, semantic search, document similarity, recommendation engines

No SQL Database¶
Scale your AI applications with enterprise-grade NoSQL storage powered by Apache Cassandra. This accelerator provides a serverless, highly available database with optional vector capabilities, perfect for storing application data, user profiles, and AI-generated content.

Key Features:

Cassandra Compatibility: Leverage proven Apache Cassandra technology in a serverless model
Vector Collections: Store and query vector embeddings alongside traditional data
Dual API Support: Use Data API for REST access or CQL for native Cassandra queries
Global Distribution: Multi-region replication for high availability and low latency
Elastic Scaling: Automatic scaling based on workload demands
Use Cases: User profile storage, session management, IoT data storage, AI application backends

Zero Copy¶
Eliminate data silos and reduce costs with federated analytics that queries data in place without copying. Built on IBM watsonx.data's open lakehouse architecture, this accelerator enables you to analyze data across multiple sources using a single query interface.

Key Benefits:

No Data Movement: Query data where it lives without ETL or replication
Cost Savings: Eliminate redundant storage and reduce infrastructure costs
Faster Insights: Access data immediately without waiting for ETL processes
Open Standards: Built on Iceberg and Delta Lake table formats for vendor independence
Unified Governance: Centralized access control and security policies
Architecture Components: - IBM watsonx.data as the query engine - Presto/Trino for distributed SQL execution - Support for S3, ADLS, and on-premises storage - Integration with Db2, PostgreSQL, and other databases

Use Cases: Multi-cloud analytics, data mesh architectures, cost optimization, real-time reporting

Getting Started¶
Quick Start Guide

Follow these steps to get started with any building block:

Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data
Navigate to the specific building block directory

Follow the README instructions for setup and configuration

Key Benefits¶
Why Use Data Building Blocks?

Faster Time-to-Value: Pre-built accelerators reduce development time
Cost Savings: Eliminate redundant storage and data movement
Enhanced Security: Built-in governance and data protection
Scalability: Optimized for enterprise AI workloads
Flexibility: Modular design allows mix-and-match capabilities
IBM Products Used¶
These building blocks leverage the following IBM products:

IBM watsonx.ai: Foundation models and AI services
IBM watsonx.data: Open lakehouse platform
IBM Cloud Object Storage: Scalable object storage
IBM Db2: Enterprise database
Contributing¶
We welcome contributions! Please fork the repository, create a feature branch, and open a pull request with your changes.

Contribution Guidelines

Follow existing code style and documentation patterns
Include tests for new features
Update documentation as needed
Ensure all tests pass before submitting
License¶
This project is licensed under the Apache 2.0 License.

Integration - Building Blocks¶
Welcome to the Integration Building Blocks documentation. These accelerators focus on data ingestion and pipeline automation to bring data into your systems.

Overview¶
Integration capabilities provide the foundation for data movement, enabling seamless ingestion of both structured and unstructured data from various sources into your data platform.

Available Building Blocks¶
Data Pipeline (AI Generated)¶
AI-powered data pipeline generation and automation for IBM watsonx.data covering unstructured and structured data sources.

Key Features:

AI-Powered Pipeline Generation: Automatically generate data pipelines using AI
Unstructured Data Ingestion: Process documents, PDFs, images, and media files
Structured Data Ingestion: RDBMS connectors with CDC support
Batch and Streaming: Support for both batch and real-time ingestion
Integration: Seamless integration with IBM watsonx.data
IBM Products:

IBM watsonx.data
IBM watsonx.ai
IBM Cloud Object Storage (COS)
IBM UDI (Unstructured Data Ingestion)
IBM Db2
Data Streaming¶
Real-time data streaming capabilities powered by Confluent Platform for continuous data flow into AI pipelines.

Key Features:

Real-time Event Ingestion: Capture and process events as they occur
Advanced Stream Processing: ksqlDB, Kafka Streams, and Flink integration
Confluent Platform: Complete data streaming solution by Kafka creators
Schema Registry: Centralized schema management for data governance
200+ Connectors: Pre-built integrations via Kafka Connect
Products:

Confluent Platform
Confluent Cloud
Apache Kafka
IBM watsonx.data
Data Observability¶
Monitor and ensure data pipeline quality and reliability with comprehensive observability capabilities.

Key Features:

Pipeline Monitoring: Real-time pipeline execution tracking and performance metrics
Data Quality Validation: Automated quality checks and anomaly detection
Alerting System: Configurable alerts with multi-channel notifications
Integration: Native integration with IBM watsonx.data and popular orchestration tools
Products:

Databand
IBM watsonx.data
Use Cases¶
Common Integration Scenarios

Data Lake Population: Ingest diverse data sources into watsonx.data
Real-time Pipelines: Stream data from operational systems
Document Processing: Extract and index document content
Database Migration: Move data from legacy systems
Resources¶
GitHub Repository
IBM watsonx.data Documentation

Data Pipeline (AI Generated)¶
AI-powered data pipeline generation and automation for IBM watsonx.data covering unstructured and structured data sources.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Data Pipeline (AI Generated)

Overview¶
The Data Pipeline (AI Generated) building block provides an intelligent framework for automatically generating and managing data pipelines for IBM watsonx.data. It leverages AI to understand data sources, recommend optimal ingestion strategies, and automate pipeline creation, reducing manual effort and accelerating time-to-value.

IBM Products Used¶
This building block leverages the following IBM products and services:

watsonx.data: Data lakehouse platform for storing and managing ingested data
IBM Cloud Object Storage (COS): Scalable object storage for data staging and archival
IBM UDI (Unstructured Data Ingestion): Purpose-built solution for ingesting unstructured data
Db2: Relational database for structured data sources
IBM Cloud Pak for Data: Unified data and AI platform for data integration
Features¶
Unstructured Data Ingestion¶
Document processing (PDF, DOCX, TXT, HTML)
Image and media file handling
Email and messaging data extraction
Web scraping and crawling capabilities
Structured Data Ingestion¶
RDBMS connectors (DB2, PostgreSQL, MySQL, Oracle)
Data warehouse integration
CDC (Change Data Capture) pipelines
Batch and streaming ingestion modes
Architecture¶
watsonx.data
AI Pipeline Generator
Data Sources
Data Lakehouse
Cloud Object
Storage
IBM UDI
Document Processing
CDC Engine
Change Capture
AI Engine
Pipeline Generation
Unstructured Data
PDF, DOCX, Images
Structured Data
RDBMS, Warehouses
Components¶
IBM UDI (Unstructured Data Ingestion)¶
IBM UDI provides specialized capabilities for ingesting unstructured data from various sources:

Document Ingestion: Process documents in multiple formats
Media Processing: Handle images, videos, and audio files
Content Extraction: Extract text and metadata from unstructured sources
Format Conversion: Convert between different file formats
Repository Path: integration/data-pipeline-ai-generated/

Structured Data Ingestion¶
Connect to and ingest data from relational databases and data warehouses:

Database Connectors: Pre-built connectors for major RDBMS platforms
CDC Support: Real-time change data capture for incremental updates
Batch Processing: Efficient bulk data loading
Schema Mapping: Automatic schema detection and mapping
Repository Path: integration/data-pipeline-ai-generated/

Getting Started¶
Prerequisites¶
Requirements

IBM watsonx.data environment
IBM Cloud Object Storage (COS) credentials
Source system credentials (database, API keys, etc.)
Python 3.12+ installed locally
git installed locally
Installation¶
Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data/integration/data-pipeline-ai-generated/
Choose your ingestion type and navigate to the appropriate directory:

For unstructured data: cd assets/unstructured-data/
For structured data: cd assets/structured-data/

Follow the specific README instructions in each directory for setup and configuration.

Use Cases¶
Data Lake Population: Ingest diverse data sources into watsonx.data
Real-time Data Pipelines: Stream data from operational systems
Document Processing: Extract and index document content
Database Migration: Move data from legacy systems to watsonx.data
API Data Integration: Pull data from external APIs and services
Log Analytics: Ingest and analyze application and system logs
Architecture Patterns¶
Batch Ingestion Pattern¶

Source System → Staging (COS) → Transformation → watsonx.data
Streaming Ingestion Pattern¶

Source System → CDC → Real-time Processing → watsonx.data
Hybrid Pattern¶

Source System → Batch/Stream Router → Processing → watsonx.data
Best Practices¶
Ingestion Best Practices

Data Quality: Implement validation checks at ingestion time
Error Handling: Design robust retry and error recovery mechanisms
Performance: Use parallel processing for large-scale ingestion
Monitoring: Track ingestion metrics and set up alerts
Security: Encrypt data in transit and at rest
Schema Evolution: Plan for schema changes in source systems
Performance Considerations¶
Batch Size: Optimize batch sizes for your data volume
Parallelization: Use multiple workers for concurrent ingestion
Network Bandwidth: Consider network capacity for large data transfers
Resource Allocation: Allocate sufficient compute and memory resources
Incremental Loading: Use CDC for efficient incremental updates
Resources¶
GitHub Repository
Support¶
For issues or questions, please refer to the GitHub repository or open an issue

Data Streaming with Confluent¶
Enterprise-grade real-time data streaming powered by Confluent Platform - the complete data streaming solution built by the original creators of Apache Kafka.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Data Streaming

Overview¶
This building block delivers enterprise-grade data streaming capabilities through Confluent Platform, the most complete data streaming solution built by the original creators of Apache Kafka. Confluent provides real-time event ingestion and stream processing for operational and analytical use cases, enabling continuous data flow into AI pipelines with enterprise security, scalability, and advanced management features.

Confluent Platform overview

Confluent Platform offers the most advanced Kafka distribution with cloud-native capabilities, comprehensive tooling, and enterprise features that go far beyond open-source Apache Kafka.

Key Features¶
Real-time Event Ingestion: Capture and process events as they occur with millisecond latency
Advanced Stream Processing: Process data streams with ksqlDB, Kafka Streams, and Flink
Cloud-Native Architecture: Fully managed Confluent Cloud or self-managed Platform
Complete Data Streaming: Most comprehensive Kafka distribution with 200+ connectors
Schema Management: Built-in Schema Registry for data governance
Scalable Architecture: Handle high-volume data streams efficiently across distributed systems
Low Latency: Minimize delay between data generation and availability
Enterprise Security: End-to-end encryption, RBAC, and compliance features
DevOps Automation: Infrastructure as Code and automated operations
Confluent Platform - Complete Data Streaming by Kafka Creators¶
Confluent Platform is the most complete data streaming platform, built by the original creators of Apache Kafka, offering enterprise features and cloud-native capabilities.

Why Choose Confluent?¶
Built by Kafka Creators¶
Created by the team that built Apache Kafka at LinkedIn
Most advanced Kafka distribution with latest features
Industry-leading expertise and innovation
Largest Kafka community and ecosystem
Confluent Cloud - Fully Managed¶
Cloud-native, serverless Kafka service
Available on AWS, Azure, and Google Cloud
Elastic scaling with pay-as-you-go pricing
99.99% uptime SLA with multi-region support
Advanced Stream Processing¶
ksqlDB: SQL-based stream processing for real-time applications
Kafka Streams: Native stream processing library
Flink on Confluent: Advanced stateful stream processing
Real-time data transformations and aggregations
Data Governance and Integration¶
Schema Registry: Centralized schema management with versioning
Kafka Connect: 200+ pre-built connectors for data integration
Stream Lineage: Track data flow across your organization
Data Quality Rules: Ensure data integrity in real-time
Enterprise Operations¶
Control Center: Advanced monitoring and management UI
Cluster Linking: Multi-datacenter replication
Tiered Storage: Cost-effective long-term data retention
Self-Balancing Clusters: Automated partition rebalancing
DevOps and Automation¶
Infrastructure as Code with Terraform
GitOps workflows for configuration management
Automated cluster provisioning and scaling
CI/CD integration for stream processing applications
Security¶
End-to-end encryption (TLS/SSL)
RBAC (Role-Based Access Control)
SASL/SCRAM, OAuth, and mTLS authentication
Audit logs and compliance reporting
Use Cases¶
Real-time analytics and data warehousing
Event-driven microservices
Customer 360 and personalization
Fraud detection and security monitoring
IoT data processing at scale
Apache Kafka - Open Source Foundation¶
Apache Kafka is the foundational distributed streaming platform that powers Confluent Platform.

Core Capabilities¶
High-throughput, low-latency message broker
Distributed, fault-tolerant architecture
Horizontal scalability to handle trillions of events
Persistent storage with configurable retention
Producer and consumer APIs for all major languages
Open-source with active community support
Use Cases¶
Real-Time Analytics¶
Process streaming data for immediate insights and decision-making with sub-second latency.

Event-Driven Applications¶
Build responsive applications that react to events in real-time using event-driven architectures.

Data Pipeline Integration¶
Feed real-time data into AI/ML pipelines for continuous model updates and real-time predictions.

Operational Monitoring¶
Monitor systems and applications with real-time data streams for proactive issue detection.

Change Data Capture (CDC)¶
Capture and stream database changes in real-time for data synchronization and replication.

Log Aggregation¶
Collect and aggregate logs from distributed systems for centralized analysis.

Architecture¶
The Data Streaming building block integrates with multiple platforms and services:

Core Streaming Platforms¶
Apache Kafka: Foundation for distributed streaming
IBM Event Streams: Enterprise Kafka on IBM Cloud
Confluent Platform: Advanced Kafka with enterprise features
Confluent Cloud: Fully managed cloud-native Kafka
Integration Points¶
IBM watsonx.data: For data lake integration and storage
IBM watsonx.ai: For AI model integration and real-time inference
Kafka Connect: For connecting to various data sources and sinks
Schema Registry: For data governance and schema evolution
Stream Processing¶
Kafka Streams: Native stream processing library
ksqlDB: SQL-based stream processing
Apache Flink: Advanced stream processing (optional)
Getting Started¶
Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data/integration/data-streaming
Choose your streaming platform:

Apache Kafka (open source)
IBM Event Streams (IBM Cloud)
Confluent Platform (self-managed)
Confluent Cloud (fully managed)

Follow the setup instructions in the README for your chosen platform

Configure your streaming sources and destinations

Deploy and monitor your streaming pipelines

Products and Services¶
IBM Products¶
IBM Event Streams: Enterprise Kafka service on IBM Cloud
IBM watsonx.data: Open lakehouse platform for data storage
IBM watsonx.ai: AI platform for model deployment
Open Source¶
Apache Kafka: Distributed streaming platform
Kafka Streams: Stream processing library
Kafka Connect: Data integration framework
Confluent¶
Confluent Platform: Complete data streaming platform
Confluent Cloud: Fully managed cloud-native Kafka
ksqlDB: Stream processing database
Schema Registry: Schema management service
Best Practices¶
Performance Optimization¶
Configure appropriate partition counts for parallelism
Tune producer and consumer settings for throughput
Use compression for network efficiency
Monitor lag and adjust consumer groups
Data Governance¶
Implement schema registry for data contracts
Use topic naming conventions
Apply access controls and authentication
Enable audit logging
Reliability¶
Configure replication factor for fault tolerance
Implement proper error handling and retry logic
Monitor cluster health and performance
Plan for disaster recovery
Related Building Blocks¶
Data Pipeline (AI Generated): Automated data pipeline generation
Data Quality: Enhance streaming data quality
Vector Search: Real-time vector search capabilities
Text2SQL: Query streaming data with natural language

Data Observability¶
Monitor and ensure data pipeline quality and reliability with comprehensive observability capabilities powered by Databand.

Overview¶
Data Observability provides comprehensive monitoring, alerting, and quality validation for data pipelines. This building block helps teams detect, diagnose, and resolve data quality issues before they impact downstream applications and AI models.

Data Observability Overview

Key Features¶
Pipeline Monitoring¶
Real-time pipeline execution tracking
Performance metrics and bottleneck identification
Historical trend analysis
Custom dashboards and visualizations
Data Quality Validation¶
Automated data quality checks
Schema validation and drift detection
Data freshness monitoring
Anomaly detection
Alerting and Notifications¶
Configurable alert rules
Multi-channel notifications (email, Slack, PagerDuty)
Incident management integration
SLA monitoring and reporting
Integration Capabilities¶
Native integration with IBM watsonx.data
Support for popular orchestration tools (Airflow, Databricks)
API-first architecture for custom integrations
Metadata collection and lineage tracking
IBM Products¶
Databand: Data observability and pipeline monitoring platform
IBM watsonx.data: Open lakehouse platform
IBM Cloud Object Storage: Scalable object storage
Use Cases¶
Common Observability Scenarios

Pipeline Health Monitoring: Track pipeline execution status and performance
Data Quality Assurance: Validate data quality before AI consumption
Incident Response: Quickly identify and resolve data issues
Compliance Reporting: Generate audit trails and compliance reports
Getting Started¶
Prerequisites¶
IBM watsonx.data instance
Databand account or installation
Access to data pipelines and sources
Quick Start¶
Configure Databand Integration


# Set up Databand connection
export DATABAND_URL="your-databand-url"
export DATABAND_TOKEN="your-api-token"
Install Databand SDK


pip install dbnd
Instrument Your Pipeline


from dbnd import task, pipeline

@task
def process_data(input_path: str) -> str:
    # Your data processing logic
    return output_path

@pipeline
def data_pipeline():
    result = process_data("/path/to/data")
    return result
Monitor Pipeline Execution

Access Databand dashboard
View pipeline runs and metrics
Configure alerts and notifications
Architecture¶
Databand Platform
Metadata & Metrics
Metadata & Metrics
Metadata & Metrics
Insights & Alerts
Insights & Alerts
Insights & Alerts
Monitoring
Engine
Quality
Checks
Alerting
System
Data Pipelines
Airflow, Databricks, Custom Scripts
Users & Downstream Systems
Dashboards, Notifications, Incident Management
Best Practices¶
Define Quality Metrics Early: Establish data quality standards before pipeline deployment
Set Appropriate Alert Thresholds: Balance between noise and missing critical issues
Monitor Data Freshness: Track data arrival times and processing delays
Document Pipeline Dependencies: Maintain clear lineage and dependency maps
Regular Review: Periodically review and update monitoring rules
Resources¶
GitHub Repository
Databand Documentation
IBM watsonx.data Documentation
Support¶
For issues or questions, please refer to the GitHub repository or contact IBM support.

Intelligence - Building Blocks¶
Welcome to the Intelligence Building Blocks documentation. These accelerators focus on data quality, governance, and lineage tracking to ensure trustworthy and timely data.

Overview¶
Intelligence capabilities provide data governance, quality controls, and lineage tracking features that ensure data is properly managed, secured, and traceable for AI and analytics workloads.

Available Building Blocks¶
Data Quality¶
Ensure data quality through validation rules and quality checks to maintain trustworthy data for AI applications.

Key Features:

Data Quality Validation: Validation rules and quality checks
Quality Monitoring: Continuous quality assessment
Data Profiling: Automated data profiling and assessment
Integration: Native integration with watsonx.data Intelligence
IBM Products:

IBM watsonx.data Intelligence
IBM Governance and Catalog
IBM Data Quality
Data Lineage¶
Track data transformations and flow across your data ecosystem for compliance and governance.

Key Features:

End-to-End Lineage: Track data flow from source to destination
Transformation Tracking: Monitor data transformations and business logic
Impact Analysis: Assess downstream impact of data changes
Compliance: Audit trails for regulatory compliance
IBM Products:

IBM watsonx.data Intelligence
IBM Governance and Catalog
IBM Manta
Text2SQL¶
Convert natural language questions into executable SQL queries using AI-powered query generation.

Key Features:

Natural Language Understanding: Interpret complex questions
SQL Generation: Generate optimized SQL queries
Schema Intelligence: Automatic schema understanding
Multi-Database Support: Support for various database systems
IBM Products:

IBM watsonx.ai
IBM watsonx.data
IBM Db2
Use Cases¶
Common Intelligence Scenarios

Data Quality Assurance: Validate and cleanse data before AI consumption
Lineage Tracking: Track data flow for compliance and governance
Impact Analysis: Understand downstream effects of data changes
Natural Language Queries: Enable non-technical users to query data
Resources¶
GitHub Repository
IBM watsonx.data Documentation
Support¶

Data Quality¶
Ensure data quality through validation rules and quality checks to maintain trustworthy data for AI applications.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Data Quality

Overview¶
The Data Quality building block provides comprehensive data quality assessment, monitoring, and validation capabilities. It enables organizations to maintain high data quality standards through automated validation rules, profiling, and continuous monitoring.

Data Quality Overview

IBM Products Used¶
This building block leverages the following IBM products and services:

IBM watsonx.data Intelligence: AI-powered data intelligence and governance platform
IBM Knowledge Catalog: Enterprise catalog for data governance
IBM Cloud Pak for Data: Unified data and AI platform
Features¶
Data Quality Management¶
Automated data quality assessment
Data profiling and validation
Quality rule definition and enforcement
Quality metrics and reporting
Data Lineage Tracking¶
End-to-end data lineage visualization
Impact analysis for data changes
Dependency tracking across systems
Automated lineage capture
Governance Integration¶
Integration with data catalogs
Policy enforcement and compliance
Audit trail and change tracking
Metadata management
Use Cases¶
Data Quality Monitoring: Continuously monitor data quality across systems
Regulatory Compliance: Track data lineage for compliance requirements
Impact Analysis: Understand downstream impacts of data changes
Data Governance: Enforce data quality standards and policies
Root Cause Analysis: Trace data issues back to their source
Getting Started¶
Prerequisites¶
Requirements

IBM watsonx.data Intelligence environment
IBM Cloud account with appropriate permissions
Python 3.12+ for automation scripts
Access to data sources for lineage tracking
Basic Setup¶
Set up watsonx.data Intelligence environment

Configure data quality rules and policies

Enable lineage tracking for data sources

Set up monitoring and alerting

Architecture Pattern¶
Governance
Data Catalog
Policies
Reports
Quality & Lineage
Data Profiling
Quality Rules
Lineage Tracking
Data Sources
Databases
Files
APIs
Best Practices¶
Quality & Lineage Best Practices

Automated Profiling: Regularly profile data to detect quality issues
Clear Rules: Define clear, measurable data quality rules
Lineage Capture: Automate lineage capture at all integration points
Impact Analysis: Perform impact analysis before making changes
Documentation: Document data quality standards and lineage
Monitoring: Set up alerts for quality threshold violations
Coming Soon¶
Upcoming Features

Detailed implementation guides
Sample quality rules and templates
Advanced lineage visualization
Machine learning-based quality prediction
Integration with additional data sources
Resources¶
IBM watsonx.data Intelligence Documentation
IBM Knowledge Catalog Documentation
GitHub Repository
Support¶
For issues or questions, please refer to the GitHub repository or contact IBM support.

Data Lineage¶
Track data transformations and flow across your data ecosystem for compliance, governance, and impact analysis.

Overview¶
Data Lineage provides end-to-end visibility into how data moves and transforms across your organization. This building block helps teams understand data origins, track transformations, assess impact of changes, and maintain compliance with regulatory requirements.

Data Lineage Overview

Key Features¶
End-to-End Lineage Tracking¶
Automatic lineage capture from data pipelines
Cross-system lineage visualization
Column-level lineage tracking
Historical lineage analysis
Transformation Tracking¶
Track data transformations and business logic
Document data quality rules and validations
Monitor schema changes and evolution
Capture metadata at each transformation step
Impact Analysis¶
Assess downstream impact of data changes
Identify affected reports and dashboards
Trace data dependencies across systems
Root cause analysis for data issues
Compliance and Governance¶
Audit trail for regulatory compliance
Data classification and sensitivity tracking
Access control and usage monitoring
Automated compliance reporting
IBM Products¶
IBM watsonx.data Intelligence: Data governance and lineage tracking
IBM Governance and Catalog: Enterprise metadata management
IBM Manta: Automated data lineage solution
Use Cases¶
Common Lineage Scenarios

Regulatory Compliance: Track data for GDPR, CCPA, and other regulations
Impact Analysis: Understand downstream effects before making changes
Data Quality: Trace data quality issues to their source
Migration Planning: Map data flows for system migrations
Architecture¶
Outputs
Lineage Platform
Data Pipelines
Data Sources
Metadata
Metadata
Reports & Dashboards
Compliance Reports
Impact Alerts
Lineage Capture
Metadata Store
Impact Analysis
Visualization
ETL Jobs
Transformations
Database 1
Database 2
File Systems
Getting Started¶
Prerequisites¶
IBM watsonx.data Intelligence or IBM Manta
Access to data sources and pipelines
Metadata collection enabled
Quick Start¶
Configure Lineage Collection


# lineage-config.yaml
lineage:
  enabled: true
  capture_level: column
  sources:
    - type: watsonx.data
      connection: wxd-prod
    - type: db2
      connection: db2-warehouse
Enable Automatic Lineage Capture


from ibm_watsonx_data import LineageTracker

tracker = LineageTracker(config="lineage-config.yaml")

# Lineage is automatically captured during pipeline execution
@tracker.track_lineage
def transform_data(source_table, target_table):
    # Your transformation logic
    pass
Query Lineage Information


# Get lineage for a specific table
lineage = tracker.get_lineage(
    table="sales_summary",
    direction="upstream",  # or "downstream"
    depth=3
)

# Visualize lineage
tracker.visualize_lineage(lineage)
Perform Impact Analysis


# Analyze impact of changing a column
impact = tracker.analyze_impact(
    table="customer_data",
    column="email_address",
    change_type="schema_change"
)

print(f"Affected tables: {impact.affected_tables}")
print(f"Affected reports: {impact.affected_reports}")
Architecture¶

┌─────────────────────────────────────────────────────────────┐
│                    Data Sources                              │
│  (Databases, Data Lakes, APIs, Files)                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Metadata & Lineage
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              Lineage Collection Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Scanners   │  │  Extractors  │  │  Parsers     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Lineage Graph
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              Lineage Repository                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Graph DB    │  │  Metadata    │  │  Analytics   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Lineage APIs & Visualizations
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  Consumption Layer                           │
│  (Dashboards, Reports, Impact Analysis, Compliance)         │
└─────────────────────────────────────────────────────────────┘
Lineage Levels¶
Table-Level Lineage¶
Tracks relationships between tables and datasets:


source_table → transformation → target_table
Column-Level Lineage¶
Tracks how individual columns are derived:


source.column_a + source.column_b → target.calculated_field
Job-Level Lineage¶
Tracks data flows through processing jobs:


job_1 → intermediate_data → job_2 → final_output
Best Practices¶
Enable Automatic Capture: Use automated lineage collection whenever possible
Document Business Logic: Add business context to technical lineage
Regular Validation: Periodically validate lineage accuracy
Access Control: Implement appropriate security for sensitive lineage data
Performance Optimization: Balance lineage detail with system performance
Integration Examples¶
With Data Pipelines¶

from ibm_watsonx_data import Pipeline, LineageTracker

pipeline = Pipeline("customer_analytics")
tracker = LineageTracker()

@pipeline.task
@tracker.track_lineage
def extract_customers(source_db):
    return source_db.query("SELECT * FROM customers")

@pipeline.task
@tracker.track_lineage
def transform_customers(raw_data):
    # Transformation logic
    return transformed_data

@pipeline.task
@tracker.track_lineage
def load_customers(data, target_db):
    target_db.insert("customer_summary", data)
With Data Quality¶

from ibm_watsonx_data import DataQuality, LineageTracker

quality = DataQuality()
tracker = LineageTracker()

# Link quality checks with lineage
@quality.check("email_format")
@tracker.track_lineage
def validate_email(data):
    # Validation logic
    return validated_data
Resources¶
GitHub Repository
IBM Manta Documentation
IBM watsonx.data Intelligence Documentation

Text2SQL¶
Convert natural language questions into executable SQL queries using AI-powered query generation with IBM watsonx.

Overview¶
Text2SQL enables users to query databases using natural language instead of writing SQL code. This building block leverages IBM watsonx.ai foundation models to understand user intent and generate accurate, optimized SQL queries that can be executed against various database systems.

Text2SQL Overview

Key Features¶
Natural Language Understanding¶
Interpret complex natural language questions
Handle ambiguous queries with context awareness
Support for multiple languages
Conversational query refinement
SQL Generation¶
Generate syntactically correct SQL queries
Support for complex joins and subqueries
Optimize query performance
Handle multiple database dialects (PostgreSQL, MySQL, Db2, etc.)
Schema Intelligence¶
Automatic schema understanding
Table and column relationship inference
Semantic mapping of business terms to database objects
Context-aware query generation
Query Validation¶
Syntax validation before execution
Security checks to prevent SQL injection
Query complexity analysis
Result set size estimation
IBM Products¶
IBM watsonx.ai: Foundation models for natural language understanding
IBM watsonx.data: Query execution and data access
IBM Db2: Enterprise database support
Presto/Trino: Distributed SQL query engine
Use Cases¶
Common Text2SQL Scenarios

Business Intelligence: Enable non-technical users to query data
Data Exploration: Quick ad-hoc analysis without SQL knowledge
Report Generation: Natural language report requests
Data Quality: Ask questions about data completeness and accuracy
Getting Started¶
Prerequisites¶
IBM watsonx.ai instance with API access
Database connection (Db2, PostgreSQL, MySQL, etc.)
Database schema metadata
Python 3.8 or higher
Quick Start¶
Install Dependencies


pip install ibm-watsonx-ai sqlalchemy
Configure Text2SQL


from ibm_watsonx_ai import Text2SQL
from sqlalchemy import create_engine

# Initialize Text2SQL
text2sql = Text2SQL(
    api_key="your-watsonx-api-key",
    project_id="your-project-id",
    model_id="meta-llama/llama-3-70b-instruct"
)

# Connect to database
engine = create_engine("postgresql://user:pass@host:5432/dbname")
text2sql.connect(engine)
Generate and Execute Queries


# Ask a question in natural language
question = "What were the top 5 products by revenue last quarter?"

# Generate SQL query
result = text2sql.query(question)

print(f"Generated SQL: {result.sql}")
print(f"Explanation: {result.explanation}")
print(f"Results:\n{result.data}")
Advanced Usage with Schema Context


# Provide schema context for better results
schema_context = {
    "tables": {
        "sales": "Contains transaction records",
        "products": "Product catalog with pricing",
        "customers": "Customer information"
    },
    "relationships": [
        "sales.product_id -> products.id",
        "sales.customer_id -> customers.id"
    ]
}

text2sql.set_schema_context(schema_context)

# Now queries will be more accurate
result = text2sql.query(
    "Show me customers who bought more than $1000 worth of electronics"
)
Architecture¶
Text2SQL Engine
Natural Language Question
SQL Query
Query Results
NL Parser
Schema Mapper
SQL Generator
Validator
Optimizer
User Interface
Natural Language Input
Database
PostgreSQL, Db2, MySQL
Result Formatter
Tables, Charts, Explanations
Example Queries¶
Simple Queries¶

# Count records
text2sql.query("How many customers do we have?")
# Generated: SELECT COUNT(*) FROM customers

# Filter and sort
text2sql.query("Show me the 10 most expensive products")
# Generated: SELECT * FROM products ORDER BY price DESC LIMIT 10
Complex Queries¶

# Aggregation with grouping
text2sql.query("What is the average order value by customer segment?")
# Generated: 
# SELECT c.segment, AVG(o.total_amount) as avg_order_value
# FROM customers c
# JOIN orders o ON c.id = o.customer_id
# GROUP BY c.segment

# Time-based analysis
text2sql.query("Compare monthly sales for this year vs last year")
# Generated:
# SELECT 
#   DATE_TRUNC('month', order_date) as month,
#   SUM(CASE WHEN YEAR(order_date) = YEAR(CURRENT_DATE) 
#       THEN total_amount ELSE 0 END) as current_year,
#   SUM(CASE WHEN YEAR(order_date) = YEAR(CURRENT_DATE) - 1 
#       THEN total_amount ELSE 0 END) as last_year
# FROM orders
# GROUP BY DATE_TRUNC('month', order_date)
Best Practices¶
Provide Schema Context: Include table descriptions and relationships for better accuracy
Start Simple: Begin with straightforward questions and gradually increase complexity
Review Generated SQL: Always review generated queries before execution in production
Use Query Limits: Add result limits to prevent overwhelming responses
Cache Common Queries: Store frequently used query patterns for faster response
Monitor Performance: Track query generation time and database execution time
Security Considerations¶
SQL Injection Prevention: All queries are validated and parameterized
Access Control: Respect database user permissions
Query Complexity Limits: Prevent resource-intensive queries
Audit Logging: Track all generated and executed queries
Data Masking: Apply data privacy rules to query results
Integration Examples¶
With Jupyter Notebooks¶

from ibm_watsonx_ai import Text2SQL
import pandas as pd

text2sql = Text2SQL(api_key="...", project_id="...")
text2sql.connect(engine)

# Use in notebook
def ask(question):
    result = text2sql.query(question)
    print(f"SQL: {result.sql}\n")
    return pd.DataFrame(result.data)

# Interactive querying
df = ask("What are the top selling products this month?")
df.plot(kind='bar')
With Web Applications¶

from flask import Flask, request, jsonify
from ibm_watsonx_ai import Text2SQL

app = Flask(__name__)
text2sql = Text2SQL(api_key="...", project_id="...")

@app.route('/query', methods=['POST'])
def query():
    question = request.json['question']
    result = text2sql.query(question)
    return jsonify({
        'sql': result.sql,
        'data': result.data,
        'explanation': result.explanation
    })
Demo Videos¶
Text-to-SQL Demo¶
Watch the Text-to-SQL application convert natural language questions into executable SQL queries:

Demo Highlights:

Natural language question input
SQL query generation using IBM watsonx.ai
Query execution and result display
Error handling and query refinement
RAG Accelerator Demo¶
Watch the RAG Accelerator demonstrate document-based question answering:

Demo Highlights:

Document ingestion and processing
Semantic search and retrieval
Context-aware question answering
Integration with watsonx.ai
Resources¶
GitHub Repository
IBM watsonx.ai Documentation
SQL Best Practices Guide

Automation Core¶
Welcome to the Automation Building Blocks documentation. This collection provides ready-to-use accelerators organized into three main categories: Build & Deploy, Optimize, and Secure.

Automation Core provides a comprehensive framework for building, optimizing, and securing enterprise applications and infrastructure. By combining intelligent automation, operational excellence, and robust security capabilities, organizations can accelerate delivery, reduce operational overhead, and maintain resilience across hybrid cloud environments.

Automation Core

Build and Deploy¶
Accelerate application delivery with standardized integration, infrastructure provisioning, and AI-assisted code modernization. Transform legacy systems into cloud-native architectures while maintaining consistency and security across all environments.

Infrastructure as Code¶
Automate infrastructure provisioning and management with declarative configuration and version-controlled templates.

Infrastructure Provisioning: Automated deployment of compute, storage, and networking resources
Hybrid / Multi-cloud Ready: Consistent provisioning across AWS, Azure, GCP, and on-premises environments
Bob-Generated IaC: Natural language prompts transformed into Terraform and Ansible configurations
Configuration Management: Drift detection and automated remediation
Environment Standardization: Repeatable, consistent infrastructure across dev, test, and production
iPaaS Integration¶
Connect applications, data, and business processes across hybrid cloud environments with IBM webMethods.

API-led Integration: Expose and consume APIs across distributed systems
Event-driven Workflows: Real-time data synchronization and event processing
Business Process Automation: Orchestrate complex workflows spanning multiple systems
Hybrid Integration: Seamlessly connect cloud and on-premises applications
Bob-Designed Integration Flows: AI-assisted integration pattern generation and workflow design
Code Modernization¶
Transform legacy applications and middleware to modern, cloud-native architectures with AI-powered tools.

Legacy to Microservices Transformation: Decompose monolithic applications into scalable microservices
Automated Code Refactoring: Modernize COBOL, mainframe, and legacy Java applications
Dependency and Library Modernization: Update frameworks, libraries, and runtime environments
Containerization: Package applications for Kubernetes and OpenShift deployment
Technical Debt Elimination: Systematic removal of outdated patterns and practices
Explore Build and Deploy →

Optimize¶
Continuously improve cost efficiency, operational stability, and resource utilization through intelligent automation. Gain financial visibility, automate resilience, and optimize resource allocation to ensure applications remain performant and economically sustainable.

Automated Resilience¶
Proactively identify and remediate vulnerabilities, compliance gaps, and operational risks with IBM Concert.

Vulnerability Detection & Correlation: Continuous CVE monitoring and impact analysis
Risk-Based Prioritization: Intelligent ranking of security and operational risks
Continuous Compliance & Posture Management: Automated compliance monitoring and drift detection
Certificate Lifecycle Management: Automated certificate renewal and expiration tracking
Dependency Risk Mapping: Identify and assess supply chain vulnerabilities
Systemic Resilience Analysis: Detect weaknesses before they impact business-critical workloads
Automated Resource Management¶
Optimize application performance and infrastructure costs with intelligent, real-time resource allocation using IBM Turbonomic.

Cost-Efficient Operations: Balance performance requirements with infrastructure spend
Real-time Scaling Decisions: Automated resource allocation based on workload demand
Performance Assurance: Prevent bottlenecks and ensure SLA compliance
Workload Placement Optimization: Intelligent scheduling across hybrid cloud infrastructure
Container Density Optimization: Maximize resource utilization in Kubernetes environments
Automated Performance Remediation: Self-healing infrastructure adjustments
FinOps¶
Gain financial transparency and cost intelligence for cloud investments with IBM Apptio.

Cloud & Infrastructure Cost Visibility: Granular tracking of spending across all cloud providers
Cost Allocation & Chargeback: Accurate attribution of costs to teams, projects, and business units
Forecasting & Budgeting: Predictive analytics for capacity planning and budget management
Cost-Aware Automation Insights: Recommendations for optimization opportunities
Unit Economics Analysis: Understand cost per transaction, user, or business outcome
Spend Anomaly Detection: Identify and alert on unexpected cost increases
Explore Optimize →

Secure¶
Protect enterprise applications, data, and infrastructure through comprehensive identity management, secrets management, and quantum-safe cryptographic capabilities. Implement robust authentication and prepare for post-quantum security threats while maintaining compliance.

Non-human Identity¶
Centralize identity, access control, secrets management, and security enforcement across hybrid environments with IBM Verify and HashiCorp Vault.

IBM Verify - Identity & Access Management: - Identity & Access Management: Unified identity governance for users and service accounts - SSO, MFA, Adaptive Access: Single sign-on with multi-factor authentication and risk-based policies - Policy Enforcement & Governance: Centralized access control and compliance enforcement - Privileged Access Management: Secure access to critical systems and sensitive data - Application Workload Security: Identity-based security for microservices and APIs - Zero Trust Architecture: Continuous verification and least-privilege access

HashiCorp Vault - Secrets Management: - Secure Storage of Secrets: Encrypted storage for API keys, passwords, and certificates - Dynamic Credentials: On-demand generation of short-lived credentials - Encryption as a Service: Centralized encryption and decryption operations - Automated Secret Rotation: Scheduled rotation of credentials without downtime - Audit Logging: Complete visibility into secret access and usage - Integration with CI/CD: Secure secret injection into deployment pipelines

Quantum Safe Cryptography¶
Prepare for post-quantum security threats with IBM Guardium Quantum Safe.

Quantum Risk Assessment: Evaluate cryptographic vulnerabilities to quantum computing attacks
Cryptographic Inventory Discovery: Automated discovery of cryptographic assets across the enterprise
Migration Roadmap Planning: Strategic planning for quantum-safe algorithm adoption
Crypto Agility Recommendations: Guidance on implementing flexible cryptographic frameworks
Post-Quantum Algorithm Implementation: Deploy NIST-approved quantum-resistant algorithms
Compliance & Regulatory Readiness: Ensure adherence to emerging quantum-safe standards
Explore Secure →

Why Automation Core?¶
Modern enterprises face increasing complexity in managing hybrid cloud environments, legacy system modernization, and evolving security threats. Automation Core addresses these challenges by:

Accelerating Delivery: Reduce manual processes and standardize deployment pipelines with IaC and iPaaS
Optimizing Operations: Balance cost, performance, and resilience through intelligent automation and FinOps
Enhancing Security: Implement robust identity management, secrets management, and quantum-safe cryptography
Enabling Transformation: Modernize legacy applications while maintaining business continuity
Ensuring Resilience: Proactively detect and remediate vulnerabilities and compliance gaps
Maximizing ROI: Optimize cloud spending and resource utilization across hybrid environments
Getting Started¶
Build and Deploy - Start with infrastructure automation, integration, and application modernization
Optimize - Implement continuous optimization for cost, performance, and resilience
Secure - Strengthen security posture with identity management, secrets management, and quantum-safe cryptography
Github Repository¶
Code for these building blocks can be found in the Automation Building Blocks repo.

Together, these building blocks create an integrated automation platform that enhances delivery speed, operational efficiency, and security posture across the entire application lifecycle—from infrastructure provisioning to quantum-safe cryptography.


Build and Deploy¶
Build and Deploy focuses on accelerating application delivery while ensuring consistency, security, and automation across environments. This building block enables organizations to standardize integration, identity, provisioning, and development workflows, reducing friction between development and operations. By combining platform services, automation frameworks, and AI-assisted tooling, enterprises can move from manual processes to repeatable, scalable delivery pipelines.

Core Capabilities¶
Capability	Technology	Description
Platform as a Service (iPaaS)	IBM webMethods	Simplify application and data integration across distributed systems
Infrastructure as Code	Terraform & Ansible	Ensure consistent, automated environment provisioning
Code Modernization	AI-powered transformation tools	Transform legacy applications and middleware to modern, cloud-native architectures
Github Repository¶
Code for these accelerators can be found in the Build - Automation Building Blocks repo.

Key Use Cases¶
Platform as a Service (IBM webMethods)¶
Organizations leverage this capability to integrate cloud and on-prem applications, orchestrate business workflows, enable API management, connect legacy systems, synchronize data across platforms, and streamline hybrid integration architectures.

Non-human Identity (IBM Verify & HashiCorp Vault)¶
Typical scenarios include centralized identity and access management, single sign-on (SSO), multi-factor authentication (MFA), adaptive access policies, privileged access control, and securing application workloads across hybrid environments.

Infrastructure as Code (Terraform, Ansible)¶
Common use cases involve automated infrastructure provisioning, environment standardization, configuration management, deployment automation, drift prevention, multi-cloud orchestration, and repeatable DevOps pipelines.

Code Modernization¶
Organizations leverage this capability to refactor legacy COBOL or mainframe code to modern languages, modernize Java applications to Spring Boot, transform monolithic applications to microservices, migrate from WebSphere/WebLogic to OpenShift, containerize legacy applications, and eliminate technical debt while maintaining business continuity.

Related Building Blocks¶
Secure - Security and cryptographic capabilities
Optimize - Continuously improve efficiency, resilience, and resource utilization
Together, these capabilities create a cohesive Build and Deploy model that enhances delivery speed, improves operational consistency, and embeds automation and security into the software lifecycle.

Platform as a Service (iPaaS)¶
← Back to Build and Deploy

Overview¶
webMethods Hybrid Integration represents a comprehensive, cloud-native Integration Platform as a Service (iPaaS) designed to connect applications, systems, and data across distributed enterprise environments. It enables organizations to seamlessly integrate SaaS applications, on-premise systems, APIs, and events through a low-code, drag-and-drop development model.

📖 Implementation Resources

For detailed implementation guides, code samples, and deployment assets, see:

iPaaS - Complete webMethods Hybrid Integration guide with implementation examples

Key Integration Patterns:

🔄 Hybrid integration (cloud + on-premise)
🔌 API management & lifecycle
📦 B2B/EDI integration
⚡ Event-driven architectures
Business Value¶
Modern enterprises operate within heterogeneous IT ecosystems where business processes span multiple platforms, cloud providers, and legacy systems. webMethods Hybrid Integration provides:

Accelerated integration development.
Reduced dependency on custom code.
Improved operational agility.
Enhanced system interoperability.
Scalable hybrid integration capabilities.
This approach allows enterprises to innovate rapidly while maintaining architectural consistency and governance.

Integration Challenges Addressed¶
Challenge	Impact
🔀 Fragmented application landscapes	Disconnected business processes
🔗 Complex SaaS-to-legacy connectivity	Integration bottlenecks
🗄️ Data silos across platforms	Inconsistent information
📡 API sprawl and governance gaps	Security and compliance risks
⚙️ Manual and brittle integration workflows	High operational overhead
👁️ Limited visibility into integration dependencies	Difficult troubleshooting
Capabilities & Functions¶
webMethods Hybrid Integration delivers a broad set of enterprise integration capabilities:

Hybrid application integration.
API lifecycle management.
Event-driven integration.
B2B / EDI integration.
Data synchronization and orchestration.
Prebuilt connectors and adapters
💡 Platform Advantage: With over 600+ pre-built connectors, the platform accelerates connectivity across cloud services, enterprise systems, databases, and messaging frameworks.

Key Features¶
Feature	Benefit
🎨 Low-code, drag-and-drop interface	Accelerate development cycles
☁️ Cloud-native scalability	Handle growing workloads
🔌 Extensive connector ecosystem	Rapid integration deployment
🛡️ Centralized integration governance	Maintain control and compliance
🔐 API management and security controls	Protect enterprise assets
⚡ Synchronous & asynchronous patterns	Flexible integration models
Example Scenarios¶
Organizations commonly leverage webMethods Hybrid Integration to:

Connect SaaS applications with core enterprise systems.
Modernize legacy integration architectures.
Expose and manage enterprise APIs.
Orchestrate cross-platform business workflows.
Enable event-driven business processes.
Integrate B2B partner ecosystems.
Synchronize distributed data sources
Operational Benefits¶
Enterprises gain:

Benefit	Outcome
⚡ Faster integration development cycles	Accelerated time-to-market
🎯 Reduced custom integration complexity	Lower maintenance burden
🔄 Improved system interoperability	Seamless data flow
📋 Enhanced architectural governance	Better compliance
📈 Scalable hybrid connectivity	Future-proof architecture
🎛️ Simplified API and event management	Operational efficiency
🎯 Strategic Impact: webMethods Hybrid Integration transforms enterprise integration from a fragmented technical challenge into a scalable, standardized platform capability.

Related Capabilities¶
Within Build and Deploy:

Non-human Identity - Secure access and identity management
Infrastructure as Code - Automated infrastructure provisioning
Quantum-Safe Cryptography - Secure integration communications
Other Building Blocks:

Code Modernization - Modernize integration platforms
Automated Resource Management - Optimize integration workloads


Infrastructure as Code¶
← Back to Build and Deploy

Automating Enterprise Retail Application Deployment¶
Modern enterprise environments demand automation that is repeatable, auditable, and scalable across both infrastructure and application layers. This architecture demonstrates a production-aligned automation model using Terraform for infrastructure provisioning and Ansible for application deployment and orchestration.

📖 Implementation Resources

For detailed implementation guides, code samples, and deployment assets, see:

Infrastructure as Code - Complete Terraform and Ansible automation guide with implementation examples

Automation Stack:

🏗️ Terraform - Infrastructure provisioning
⚙️ Ansible - Application deployment & configuration
🔄 CI/CD Integration - Continuous delivery
📋 GitOps - Version-controlled infrastructure
Business Value¶
Cloud-native platforms introduce dynamic infrastructure lifecycles and distributed workloads. Enterprises must balance agility, stability, governance, and cost efficiency. A Terraform + Ansible automation model delivers:

Consistent environment provisioning.
Reduced manual intervention.
Improved deployment reliability.
Stronger governance and auditability.
Seamless CI/CD integration.
This separation of responsibilities enables scalable and predictable automation.

Automation Challenges Addressed¶
Challenge	Solution
⚠️ Manual and error-prone provisioning	Declarative infrastructure code
🔄 Configuration drift across environments	State management & drift detection
📦 Inconsistent application deployments	Standardized playbooks
🔁 Difficulty replicating production setups	Environment templates
📋 Limited operational standardization	Codified best practices
⏱️ Slow environment creation cycles	Automated provisioning
Capabilities & Functions¶
Terraform -- Infrastructure as Code¶
Terraform provides declarative infrastructure lifecycle management, enabling:

VPC and networking creation.
OpenShift cluster provisioning.
IAM and security configuration.
Environment replication.
Drift detection and state management.
Terraform is optimized for managing infrastructure state.

Ansible -- Configuration & Orchestration¶
Ansible provides procedural automation designed for:

Application deployment.
Platform configuration.
Kubernetes/OpenShift resource management.
Day-2 operational workflows.
CI/CD pipeline execution.
Ansible is optimized for managing application and configuration state.

Enterprise Automation Strategy¶
Layer	Primary Tool	Objective
🏗️ Infrastructure	Terraform	Provision cloud & cluster resources
⚙️ Platform Configuration	Ansible	Configure namespaces, policies
📦 Applications	Ansible	Deploy workloads & services
🔄 Operations	Ansible	Continuous operational automation
💡 Key Principle: This layered strategy ensures clear separation of concerns and maintainable automation.

Infrastructure Provisioning¶
Terraform automates the creation of foundational components required to host enterprise workloads:

Virtual Private Cloud (VPC)\
Networking and security controls\
OpenShift cluster\
Worker node pools
Terraform's state-driven model ensures reproducibility, drift prevention, and auditable changes while minimizing operational risk.

Application Deployment¶
Ansible orchestrates the Retail application lifecycle, including:

Namespace creation.
Image build and registry push.
Secret and credential management.
PostgreSQL deployment.
Backend and frontend services.
Database schema initialization.
Rolling restarts.
Validation checks.
This reflects common enterprise microservices deployment patterns.

Operational Benefits¶
Enterprises gain:

Benefit	Impact
✅ Idempotent deployments	Predictable outcomes
🤖 Reduced manual intervention	Lower error rates
⚡ Faster environment creation	Accelerated delivery
🎯 Consistent platform configuration	Standardization
🔧 Simplified Day-2 operations	Operational efficiency
🚀 Improved release reliability	Higher confidence
🎯 Strategic Outcome: This automation framework demonstrates how enterprises can standardize infrastructure provisioning, automate application deployments, reduce operational risk, and improve scalability while aligning with DevOps best practices.

Summary¶
This automation framework demonstrates how enterprises can standardize infrastructure provisioning, automate application deployments, reduce operational risk, and improve scalability while aligning with DevOps best practices.

Related Capabilities¶
Within Build and Deploy:

Platform as a Service (iPaaS) - Integrate infrastructure with applications
Non-human Identity - Automate identity provisioning
Quantum-Safe Cryptography - Secure infrastructure credentials
Other Building Blocks:

Code Modernization - Modernize infrastructure patterns
Automated Resource Management - Optimize provisioned resources
Automated Resilience & Compliance - Ensure infrastructure compliance

Code Modernization¶
← Back to Build and Deploy

Overview¶
Code Modernization provides comprehensive strategies and tools for transforming legacy applications, codebases, middleware, and integration layers into modern, cloud-native architectures. It enables enterprises to refactor legacy code, migrate from monolithic platforms to microservices, containerize workloads, and adopt cloud-native patterns while preserving business functionality and improving code quality.

📖 Implementation Resources

For detailed implementation guides and modernization strategies for code and application transformation, see:

Code Modernization - Application code and middleware transformation strategies

Operational Context¶
Modern enterprises operate applications and middleware platforms built on legacy technologies, outdated programming languages, and monolithic architectures that are difficult to scale, costly to maintain, and incompatible with cloud-native architectures. Legacy codebases, application servers, message brokers, and integration middleware often represent significant technical debt and operational complexity.

Code modernization addresses these challenges by providing structured transformation paths from legacy code and platforms to modern, maintainable architectures.

Why It Matters¶
Without effective code modernization, enterprises face:

High operational costs for legacy platforms
Limited scalability and elasticity
Difficulty adopting cloud-native patterns
Vendor lock-in and licensing constraints
Slow application deployment cycles
Increased security vulnerabilities
Code modernization transforms infrastructure from a constraint into an enabler of business agility.

Enterprise Challenges Solved¶
Legacy codebases written in outdated languages or frameworks
Monolithic application architectures
Technical debt and unmaintainable code
Legacy middleware platforms with high operational costs
Complex integration patterns
Limited cloud compatibility
Slow deployment and scaling
Difficulty adopting DevOps and modern development practices
Technology Enablement -- Transformation Frameworks¶
Code Modernization is enabled by AI-powered code analysis tools, transformation frameworks, migration tools, and cloud-native platforms that automate the conversion of legacy code and middleware to modern architectures. These tools analyze existing codebases and middleware configurations, identify refactoring opportunities, generate migration plans, and automate deployment to cloud-native platforms.

The approach is designed for large-scale enterprise code and application transformations across multiple systems and platforms.

Core Capabilities¶
Code Analysis & Assessment AI-powered analysis of legacy codebases to identify technical debt, code smells, security vulnerabilities, and modernization opportunities. Automatically analyzes code structure, dependencies, and usage patterns to create comprehensive transformation plans.

Language & Framework Modernization Automated conversion and refactoring of legacy code to modern languages and frameworks (e.g., COBOL to Java, legacy Java to Spring Boot, monolithic to microservices).

Application Refactoring Enables transformation of monolithic applications into microservices architectures while preserving business logic and data integrity. Includes automated code restructuring, dependency management, and API generation.

Middleware & Platform Migration Provides structured migration strategies from legacy platforms (WebSphere, WebLogic, JBoss) to cloud-native alternatives (OpenShift, Kubernetes, serverless).

Integration Modernization Modernizes integration patterns from legacy ESB and message brokers to cloud-native integration platforms and event-driven architectures.

Automated Testing & Validation Generates automated tests to ensure modernized code maintains functional equivalence with legacy systems.

Automated Deployment Provides automated deployment pipelines for migrated applications to cloud-native platforms with proper configuration and security controls.

Representative Use Cases¶
Organizations commonly leverage code modernization to:

Refactor legacy COBOL, RPG, or mainframe code to modern languages
Modernize Java applications to Spring Boot and cloud-native frameworks
Transform monolithic applications to microservices
Migrate from WebSphere/WebLogic to OpenShift
Modernize integration middleware to cloud-native patterns
Containerize legacy applications
Adopt serverless architectures
Eliminate technical debt and improve code maintainability
Reduce middleware licensing costs
Improve application scalability and resilience
Operational Impact¶
Enterprises benefit through:

Reduced operational costs
Improved application scalability
Faster deployment cycles
Enhanced security posture
Better cloud compatibility
Increased development agility
Reduced technical debt
Strategic Outcome¶
Code Modernization transforms legacy infrastructure into a strategic asset, enabling enterprises to adopt cloud-native architectures, reduce operational costs, and accelerate application delivery while maintaining business continuity.

Related Capabilities¶
Within Build and Deploy:

Infrastructure as Code - Automate modernized infrastructure deployment
iPaaS - Integrate modernized middleware
Other Building Blocks:

Automated Resource Management - Optimize modernized workloads
FinOps - Track modernization cost benefits
Automated Resilience & Compliance - Ensure modernized workload compliance

Optimize Building Blocks¶
Optimize focuses on continuously improving cost efficiency, operational stability, and resource utilization across hybrid cloud environments. It brings together financial visibility, resilience automation, and intelligent resource management to ensure applications remain performant, compliant, and economically sustainable.

Core Capabilities¶
Capability	Technology	Description
Automated Resilience & Compliance	IBM Concert	Centralized visibility into application risk, vulnerabilities, and compliance posture
FinOps	Aptio	Financial transparency and cost intelligence for cloud investments
Automated Resource Management	IBM Turbonomic	Intelligent resource allocation ensuring performance while preventing waste
Github Repository¶
Code for these accelerators can be found in the Optimize building blocks repo

Use Cases¶
Automated Resilience & Compliance (IBM Concert)¶
Organizations leverage this capability to continuously monitor CVE exposure, detect compliance drift, manage certificate lifecycles, assess security posture, map dependency risks, and identify systemic resilience weaknesses before they affect business-critical workloads.

FinOps¶
Cost transparency across teams, budget forecasting, spend anomaly detection, granular cost allocation, unit economics analysis, ROI evaluation for cloud initiatives, and identifying optimization opportunities across multi-cloud environments.

Automated Resource Management (IBM Turbonomic)¶
Typical scenarios involve real-time resource scaling, workload placement optimization, infrastructure bottleneck prevention, SLA protection, container density optimization, automated performance remediation, and balancing cost-performance trade-offs.

Related Building Blocks¶
Build and Deploy - Automate infrastructure and application deployment
Secure - Security and cryptographic capabilities
Together, these building blocks establish a closed-loop optimization model where cost efficiency, resilience, and performance continuously inform and enhance one another.

Automated Resilience & Compliance¶
← Back to Optimize

Overview¶
Automated Resilience & Compliance focuses on continuously safeguarding application stability, security posture, and regulatory alignment across complex hybrid cloud environments. It provides enterprises with unified visibility into operational risks, vulnerabilities, and compliance deviations, enabling proactive rather than reactive management.

📖 Implementation Resources

For detailed implementation guides, code samples, and deployment assets, see:

Automated Resilience - Complete IBM Concert integration guide with implementation examples

Core Focus Areas:

🛡️ Security Posture - Continuous vulnerability monitoring
📋 Compliance Tracking - Regulatory alignment
🔐 Certificate Management - Lifecycle automation
🔗 Dependency Mapping - Risk correlation
High Level Architecture¶
image

Why It Matters for Enterprises¶
Modern enterprises operate highly distributed, containerized workloads where risks emerge dynamically --- from newly disclosed vulnerabilities to configuration drift and certificate expirations. Manual monitoring and fragmented tooling create blind spots, slow response times, and increased exposure. Continuous resilience and compliance automation reduces operational risk, strengthens governance, minimizes downtime, and supports regulatory obligations without introducing friction into delivery pipelines.

What We Do Here¶
This building block centralizes resilience and compliance intelligence using platforms such as IBM Concert, correlating security, runtime, and configuration insights. It enables organizations to detect vulnerabilities, identify systemic weaknesses, evaluate compliance posture, and prioritize remediation actions based on risk impact.

💡 Key Principle: Create actionable intelligence rather than simply generating alerts.

Key Features & Capabilities¶
Feature	Capability
👁️ Unified visibility	Applications, clusters, environments
🔍 Continuous vulnerability monitoring	CVE exposure tracking
📋 Compliance posture tracking	Drift detection
🔐 Certificate lifecycle management	Expiration prevention
🔗 Dependency & risk correlation	Service mapping
🎯 Risk-based prioritization	Remediation guidance
Core Capabilities¶
Automated Resilience & Compliance delivers continuous risk intelligence by combining operational telemetry, vulnerability data, and governance signals. It helps enterprises understand not only what is wrong, but also why it matters and where to act first. By correlating application dependencies, infrastructure conditions, and security findings, it enables more accurate impact analysis and faster decision-making.

Use Cases¶
Organizations typically adopt this capability to:

Use Case	Benefit
🔍 Continuously detect CVE exposure	Proactive security
📋 Identify compliance drift	Regulatory readiness
🔐 Prevent certificate-related outages	Service continuity
🔗 Map vulnerabilities to services	Impact assessment
🎯 Prioritize remediation by risk	Efficient resource use
✅ Improve audit readiness	Continuous compliance
🎯 Strategic Value: Automated Resilience & Compliance transforms resilience and governance from periodic audits into continuous, automated practices that reduce operational risk and strengthen enterprise security posture.

Related Capabilities¶
Within Optimize:

FinOps - Balance security investments with cost efficiency
Automated Resource Management - Ensure compliant resource allocation
Code Modernization - Modernize for better resilience
Other Building Blocks:

Non-human Identity - Strengthen identity and access controls
Quantum-Safe Cryptography - Secure cryptographic compliance
Infrastructure as Code - Ensure infrastructure compliance

FinOps¶
← Back to Optimize

Overview¶
FinOps (Financial Operations) establishes a data-driven discipline that enables organizations to manage, govern, and optimize cloud investments. As enterprises scale across hybrid and multi-cloud environments, financial visibility becomes as critical as performance monitoring.

📖 Implementation Resources

For detailed implementation guides, code samples, and IBM Bob Custom Mode for IBM Apptio, see:

FinOps - Complete IBM Apptio integration guide with IBM Bob Custom Mode for natural language financial operations

FinOps Pillars:

💰 Cost Visibility - Granular spend tracking
📊 Budget Management - Forecasting & planning
🎯 Optimization - Waste reduction
📋 Governance - Policy enforcement
Business Value¶
Cloud consumption models introduce dynamic, usage-based spending patterns that traditional budgeting approaches cannot adequately control. FinOps provides enterprises with the mechanisms to align cloud expenditure with business priorities, ensuring that innovation velocity does not compromise financial discipline.

Financial Challenges Addressed¶
Challenge	Solution
🔍 Lack of cost transparency	Granular cost allocation
📈 Unpredictable cloud spending	Budget forecasting
🏢 Difficulty attributing costs	Chargeback models
💸 Inefficient resource utilization	Waste identification
📊 Limited ROI insight	Unit economics analysis
Capabilities & Functions¶
FinOps capabilities, typically enabled through platforms such as Aptio, deliver:

Granular cost allocation and chargeback models.
Budget forecasting and spend planning.
Cost anomaly detection and variance analysis.
Unit economics and service cost modeling.
Optimization opportunity identification.
Financial governance and policy alignment.
Decision Intelligence¶
FinOps transforms raw billing data into actionable financial intelligence:

Question	Insight
📊 What drives cost fluctuations?	Trend analysis & anomaly detection
💎 Which workloads generate highest value?	ROI & unit economics
💸 Where is waste occurring?	Optimization opportunities
🏗️ How do architecture decisions impact costs?	Cost modeling & forecasting
Example Scenarios¶
Organizations commonly leverage FinOps to:

Establish cost accountability across product teams.
Optimize cloud commitments and reserved capacity.
Evaluate modernization initiatives using cost-benefit analysis.
Detect unexpected spend spikes before financial impact.
Improve budgeting accuracy for dynamic workloads.
Align cloud usage with business growth strategies.
🎯 Strategic Impact: FinOps enables enterprises to treat cloud economics as a continuous optimization process rather than a retrospective financial exercise, aligning technology investments with business value.

Related Capabilities¶
Within Optimize:

Automated Resource Management - Optimize resource costs and utilization
Automated Resilience & Compliance - Balance security investments with cost efficiency
Code Modernization - Analyze modernization cost benefits
Other Building Blocks:

Infrastructure as Code - Track infrastructure provisioning costs

Automated Resource Management¶
← Back to Optimize

Overview¶
Automated Resource Management focuses on continuously optimizing application performance and infrastructure efficiency by dynamically aligning resource allocation with real-time demand. In modern hybrid and multi-cloud environments, this capability ensures that applications receive precisely the resources they require — no more, no less.

📖 Implementation Resources

For detailed implementation guides, code samples, and IBM Bob Custom Mode for IBM Turbonomic, see:

Automated Resource Management - Complete IBM Turbonomic integration guide with IBM Bob Custom Mode for natural language resource optimization

Optimization Dimensions:

⚡ Performance - Maintain SLA compliance
💰 Cost - Eliminate waste
📊 Utilization - Maximize efficiency
🔄 Automation - Real-time decisions
Why It Matters for Enterprises¶
Enterprises frequently struggle with competing priorities: maintaining application performance while controlling infrastructure and cloud costs. Overprovisioning leads to wasted spend, while underprovisioning risks performance degradation and SLA violations. Manual tuning cannot keep pace with dynamic workloads. Automated resource management eliminates this trade-off by continuously balancing performance, utilization, and cost efficiency in real time.

What We Do Here¶
This building block leverages platforms such as IBM Turbonomic to analyze application demand, resource consumption patterns, and infrastructure constraints. It enables automated actions that optimize workload placement, scaling, and resourcing decisions.

💡 Key Principle: Not simply monitoring utilization metrics, but actively ensuring that application performance objectives are met with optimal efficiency.

Key Features & Capabilities¶
Feature	Capability
⚡ Real-time demand-driven allocation	Dynamic resource adjustment
🎯 Intelligent workload placement	Optimal infrastructure use
🛡️ Continuous performance assurance	SLA protection
🤖 Automated scaling decisions	No manual intervention
🚫 Bottleneck prevention	Proactive capacity management
💰 Cost-performance optimization	Balanced efficiency
Core Capabilities¶
Automated Resource Management delivers closed-loop automation by continuously analyzing telemetry across applications, containers, and infrastructure layers. It determines the most efficient resource actions required to maintain performance objectives. By understanding application dependencies and constraints, it avoids disruptive scaling behaviors and instead applies precise, context-aware optimizations.

Use Cases¶
Organizations typically adopt this capability to:

Use Case	Benefit
🎯 Prevent performance bottlenecks	Proactive issue prevention
💸 Eliminate overprovisioning	Cost reduction
🔄 Optimize workload placement	Efficient resource use
✅ Maintain SLA compliance	Service reliability
📦 Improve container density	Infrastructure efficiency
🤖 Automate scaling decisions	Operational efficiency
⚖️ Balance cost & performance	Optimal trade-offs
🎯 Strategic Value: Automated Resource Management transforms resource management from static provisioning into intelligent, automated decision-making that continuously balances performance, utilization, and cost efficiency in real time.

Related Capabilities¶
Within Optimize:

FinOps - Optimize costs while maintaining performance
Automated Resilience & Compliance - Ensure compliant resource allocation
Code Modernization - Optimize modernized workloads
Other Building Blocks:

Infrastructure as Code - Automate resource provisioning

Secure Building Blocks¶
Secure focuses on protecting enterprise applications, data, and infrastructure through comprehensive identity management and quantum-safe cryptographic capabilities. It enables organizations to implement robust authentication, manage cryptographic keys, and prepare for post-quantum security threats while maintaining compliance and operational efficiency.

Core Capabilities¶
Capability	Technology	Description
Non-human Identity	IBM Verify & HashiCorp Vault	Centralize identity, access control, and security enforcement
Quantum-Safe Cryptography	IBM Guardium Crypto Manager	Enterprise-grade cryptographic key management and post-quantum cryptographic capabilities
Github Repository¶
Code for these accelerators can be found in the Secure - Automation Building Blocks repo.

Key Use Cases¶
Non-human Identity (IBM Verify & HashiCorp Vault)¶
Typical scenarios include centralized identity and access management, single sign-on (SSO), multi-factor authentication (MFA), adaptive access policies, privileged access control, and securing application workloads across hybrid environments.

Quantum-Safe Cryptography (IBM Guardium Crypto Manager)¶
Organizations leverage this capability to implement post-quantum cryptographic algorithms, manage cryptographic key lifecycles, automate certificate management, ensure cryptographic compliance, and protect sensitive data against both current and future quantum computing threats.

Related Building Blocks¶
Build and Deploy - Automate infrastructure and application deployment
Optimize - Continuously improve efficiency, resilience, and resource utilization
Together, these capabilities create a comprehensive security model that protects enterprise assets through robust identity management and quantum-resistant cryptography while maintaining operational efficiency and compliance.

Non-human Identity¶
← Back to Secure

Overview¶
Non-human Identity delivers enterprise‑grade identity and access management solutions that secure user access across cloud, hybrid, and on‑premises environments. This building block provides two complementary approaches:

IBM Verify - Unified identity and access management for user authentication
HashiCorp Vault - Secrets management and machine identity authentication
Together, these solutions centralize authentication, access controls, and risk‑based decisions while enabling seamless, secure access to applications and services for both human and non-human identities.

📖 Implementation Resources

For detailed implementation guides, code samples, and deployment assets, see:

Non-human Identity - Complete IBM Security Verify integration guide with watsonx Orchestrate, watsonx.governance, and watsonx.ai

Core Security Pillars:

🔐 Centralized identity verification (human & machine)
🎯 Risk-based adaptive access
🔑 Multi-factor authentication (MFA)
🔒 Secrets and credentials management
🌐 Federation & standards support
Why It Matters¶
Identity has become the new security perimeter in modern enterprise architectures. Effective authentication management protects sensitive assets, ensures only authorized identities (both human and machine) gain access, and supports compliant, auditable access policies. Modern authentication solutions enable organizations to balance strong security with user convenience through adaptive authentication, centralized control, and automated secrets management.

Challenges Addressed¶
Modern authentication management helps solve key enterprise challenges:

Challenge	Solution
🔓 Inconsistent authentication mechanisms	Unified authentication layer
⚠️ Credential compromise risks	Multi-factor authentication & secrets rotation
📋 Fragmented access policies	Centralized policy management
🔄 Poor user experience (repeated logins)	Single sign-on (SSO)
🔐 Hardcoded secrets in applications	Dynamic secrets generation
🤖 Machine identity management	Automated credential lifecycle
📊 Compliance and audit complexity	Automated compliance reporting
Solution Components¶
IBM Verify - Identity & Access Management¶
IBM Verify provides unified identity and access management for human identities:

Centralized Identity Verification Provides a unified authentication layer across workforce and consumer applications.
Single Sign‑On (SSO) Enables users to authenticate once and securely access multiple applications.
Multi‑Factor Authentication (MFA) Strengthens identity verification using multiple authentication factors.
Adaptive & Risk‑Based Access Adjusts authentication requirements dynamically based on contextual risk signals.
Federation & Standards Support Integrates with enterprise ecosystems using industry‑standard identity protocols.
Lifecycle & Policy Controls Ensures access is governed by identity lifecycle events and organizational policies.
HashiCorp Vault - Secrets Management¶
HashiCorp Vault provides enterprise secrets management and machine identity authentication:

Dynamic Secrets Generation Generates short-lived credentials on-demand for databases, cloud platforms, and services.
Secrets Encryption & Storage Centrally stores and encrypts API keys, passwords, certificates, and tokens.
Identity-Based Access Authenticates applications and services using machine identities (Kubernetes, AWS IAM, etc.).
Automated Secrets Rotation Automatically rotates credentials to minimize exposure windows.
Encryption as a Service Provides encryption/decryption operations without exposing keys to applications.
Audit Logging Maintains detailed audit trails of all secrets access and operations.
Core Features¶
Feature	IBM Verify	HashiCorp Vault
🎯 Centralized authentication	✅ User identities	✅ Machine identities
🛡️ Risk‑adaptive security controls	✅ Context-aware	✅ Policy-based
🔐 Secrets management	➖ Basic	✅ Advanced
☁️ Cloud‑native & hybrid deployment	✅ Full support	✅ Full support
🔗 Identity federation support	✅ SAML, OIDC	✅ Multiple auth methods
🔄 Dynamic credentials	➖	✅ On-demand generation
📊 Audit and compliance reporting	✅ Comprehensive	✅ Detailed logs
🔌 Application integration	✅ SSO focus	✅ API/SDK focus
Typical Use Cases¶
IBM Verify Use Cases¶
Enable secure single sign‑on across enterprise applications
Enforce multi‑factor authentication for workforce access
Support hybrid workforce access (cloud + on-premises)
Implement Zero Trust security models for user access
Integrate SaaS and legacy systems with unified authentication
Improve regulatory compliance posture for user access
HashiCorp Vault Use Cases¶
Eliminate hardcoded credentials in application code
Generate dynamic database credentials for microservices
Manage API keys and tokens for cloud services
Automate certificate lifecycle management
Secure CI/CD pipeline credentials
Implement machine-to-machine authentication
Encrypt sensitive data in transit and at rest
Manage Kubernetes secrets and service accounts
Business Outcomes¶
Enterprises benefit through:

Outcome	Impact
🔒 Reduced unauthorized access risks	Enhanced security posture for human & machine identities
✨ Improved user access experience	Higher productivity through SSO
🔐 Eliminated credential sprawl	Centralized secrets management
📋 Stronger governance & policy consistency	Better control across all identities
✅ Enhanced compliance readiness	Comprehensive audit trails
🤖 Automated secrets lifecycle	Reduced operational overhead
📈 Scalable hybrid identity integration	Future-proof architecture
⚡ Faster incident response	Rapid credential rotation
🎯 Strategic Value: Modern authentication management transforms identity and secrets management into a centralized, adaptive, and intelligence‑driven capability that strengthens enterprise security for both human and machine identities while preserving productivity.

Summary¶
Non-human Identity provides comprehensive identity and access control through two complementary solutions:

IBM Verify delivers centralized, adaptive user authentication with SSO, MFA, and risk-based access controls
HashiCorp Vault provides enterprise secrets management with dynamic credentials, automated rotation, and machine identity authentication
Together, they create a complete authentication framework that strengthens enterprise security while preserving user productivity and enabling secure automation.

Related Capabilities¶
Within Secure:

Quantum-Safe Cryptography - Cryptographic key management
Other Building Blocks:

Platform as a Service (iPaaS) - Secure application integration
Infrastructure as Code - Automated infrastructure with identity controls
Code Modernization - Modernize authentication middleware
Automated Resilience & Compliance - Continuous security posture monitoring

Quantum-Safe¶
← Back to Secure

Overview¶
IBM Quantum Safe Explorer is a developer-focused tool that scans application source code and binaries to discover cryptographic assets and vulnerabilities. It helps organizations understand where cryptography is being used and identify algorithms that may become vulnerable in the quantum era. It can generate inventories in several formats, including a Cryptography Bill of Materials (CBOM).

📖 Implementation Resources

For detailed implementation guides, code samples, and deployment assets, see:

Quantum-Safe - Complete IBM Quantum Safe Explorer integration guide with IBM Bob for quantum-resistant cryptography

Typical Discoveries¶
IBM Quantum Safe Explorer identifies:

Encryption algorithms (RSA, ECC, AES, SHA, etc.)
Key sizes and modes
Cryptographic libraries
Certificates and protocols
Locations in code where cryptography is implemented
Quantum-vulnerable algorithms
Who Should Use Quantum Safe Explorer?¶
Target Personas¶
IBM Quantum Safe Explorer is designed for technology leaders who need to ensure their products and infrastructure are prepared for the quantum era:

VP of Products - Product leaders responsible for ensuring their software products remain secure and competitive as quantum computing advances
Chief Information Security Officer (CISO) - Security executives tasked with protecting organizational assets and maintaining cryptographic compliance
Ideal for Software Companies¶
This solution is particularly valuable for software companies across various sectors:

SaaS Providers - Cloud-based software platforms requiring robust cryptographic security
Database Vendors - Companies providing data storage solutions with encryption requirements
CRM & ERP Systems - Enterprise software managing sensitive business data
HR Platforms - Human resources systems handling confidential employee information
AI & Machine Learning Companies - Organizations building AI solutions that require secure data processing
IBM Client Zero Success Story¶
IBM has successfully implemented Quantum Safe Explorer internally as part of its "Client Zero" initiative, demonstrating the solution's effectiveness in real-world enterprise environments. This internal deployment has enabled IBM to accelerate crypto-agility across its product portfolio and prepare for the post-quantum era.

Learn more about IBM's journey: Empowering CIOs to Accelerate Crypto-Agility with IBM Quantum Safe Explorer

What is a CBOM?¶
A Cryptography Bill of Materials (CBOM) provides a standardized inventory of the cryptographic assets used within software and systems, including algorithms, keys, certificates, protocols, and their configurations. As a core capability of the CycloneDX standard, CBOM gives organizations visibility into how and where cryptography is deployed across their environments. This visibility enables security teams to identify vulnerable or deprecated cryptographic components, support compliance requirements, and improve cryptographic agility. CBOM also plays a critical role in helping organizations assess and prepare for the transition to post-quantum cryptography.

IBM's Leadership in CBOM Standardization¶
IBM is at the forefront of advancing the adoption and standardization of the Cryptography Bill of Materials (CBOM), a critical capability within the CycloneDX standard. CBOM provides comprehensive visibility into cryptographic assets, including algorithms, keys, certificates, and protocols. This detailed inventory enables organizations to identify vulnerable or deprecated cryptography, enforce security policies, and prepare for the transition to post-quantum cryptography.

IBM Research has been instrumental in defining the CycloneDX CBOM specification and driving its industry adoption. In 2024, IBM open-sourced CBOMkit, a powerful toolkit that enables cryptographic inventory generation, visualization, analysis, and storage. To further advance industry collaboration, IBM contributed these capabilities to the Post-Quantum Cryptography Alliance (PQCA) under the Linux Foundation.

As a core capability of the CycloneDX standard, CBOM empowers organizations to:

Discover and inventory cryptographic assets across their application portfolio
Identify vulnerable or obsolete algorithms that pose security risks
Assess exposure to emerging quantum threats and quantum-vulnerable cryptography
Support security governance and regulatory compliance requirements
Prioritize and plan cryptographic migration initiatives for quantum readiness
CBOM Contents¶
Information	Example
Algorithms	RSA-2048, AES-256, SHA-1
Libraries	OpenSSL, BouncyCastle
Protocols	TLS 1.2
Certificates	X.509 certificates
Key sizes	1024-bit, 2048-bit
Dependencies	Which components use which crypto
IBM Quantum Safe Explorer automatically generates CBOMs in JSON format whenever a scan is performed.

Using Quantum Safe Explorer in a CI/CD Pipeline¶
Continuously scan applications during CI/CD using IBM Quantum Safe Explorer to automatically create CBOMs, identify vulnerable cryptography, and use IBM BOB Building Blocks to generate code fixes and modernize those cryptographic implementations so applications become crypto-agile and prepared for the post-quantum era.

End-to-End Flow¶

Developer
    ↓
Git Push
    ↓
CI/CD Pipeline
    ↓
Build + Tests
    ↓
IBM Quantum Safe Explorer
    ↓
Generate CBOM
    ↓
Detect weak algorithms
    ↓
IBM BOB
    ↓
Code remediation suggestions
    ↓
Pull Request created
    ↓
Developer approval
    ↓
Re-scan with Explorer
    ↓
Updated CBOM
    ↓
Application becomes Quantum Ready
Step-by-Step Process¶
Step 1: Developer pushes code¶
Code is committed to version control systems: - GitHub / GitLab - Triggers CI/CD Pipeline

CI/CD Platform Examples: - GitHub Actions - Jenkins - Tekton - Azure DevOps

Step 2: Run IBM Quantum Safe Explorer¶
During the pipeline execution:


Build
 ↓
Unit Tests
 ↓
Quantum Safe Explorer Scan
 ↓
Generate CBOM
 ↓
Publish Artifacts
Explorer scans the source code and produces: - findings.json - CSV reports - CBOM.json

Example discovery:


{
  "algorithm": "RSA",
  "key_size": 1024,
  "location": "auth-service.java:120",
  "risk": "High"
}
Step 3: Identify vulnerable cryptography¶
Suppose the scan finds:

Algorithm	Risk
RSA-1024	High
SHA-1	High
TLS 1.0	High
ECC P-256	Medium
These become entries in the generated CBOM.

Step 4: Feed CBOM into IBM BOB¶
IBM BOB (Building Blocks) is IBM's AI-assisted engineering platform that can consume reports and code repositories and help developers modernize or remediate code.

BOB can: - Read the CBOM - Understand where weak cryptography exists - Generate pull requests - Suggest replacement APIs - Produce migration guides - Update code automatically

Step 5: Remediate cryptography¶
Example 1: Hash Algorithm Update¶
Before:


MessageDigest md = MessageDigest.getInstance("SHA-1");
BOB suggests:


MessageDigest md = MessageDigest.getInstance("SHA-256");
Example 2: RSA Key Size Update¶
Before (1024-bit keys):


KeyPairGenerator.getInstance("RSA");
BOB updates to:


KeyPairGenerator keyGen = KeyPairGenerator.getInstance("RSA");
keyGen.initialize(3072);
Example 3: TLS Protocol Update¶
Old:


TLS 1.0
Updated:


TLS 1.3
Example 4: Post-Quantum Algorithms¶
Eventually, BOB may recommend migration to NIST PQC algorithms such as: - ML-KEM (Kyber) - Key encapsulation - ML-DSA (Dilithium) - Digital signatures

Benefits¶
This integrated approach provides:

Continuous cryptographic inventory - Always know what crypto is in use
Automatic CBOM generation - No manual tracking required
Early detection of weak algorithms - Find issues before production
AI-assisted remediation - Faster fixes with BOB suggestions
Faster transition to post-quantum cryptography - Automated migration paths
Improved crypto agility - Easy algorithm updates across codebase
Getting Started¶
To implement IBM Quantum Safe Explorer in your CI/CD pipeline:

Review the complete implementation guide
Integrate IBM Quantum Safe Explorer into your CI/CD pipeline
Configure automated CBOM generation and vulnerability scanning
Set up IBM BOB for AI-assisted cryptographic remediation
Implement continuous monitoring and re-scanning after remediation
Establish workflows for pull request review and approval
Best Practices¶
Integrate Explorer Early - Add IBM Quantum Safe Explorer to CI/CD pipelines from the start
Automate CBOM Generation - Generate CBOMs automatically with every build
Start with Risk Assessment - Use Explorer to evaluate quantum vulnerability of existing cryptographic implementations
Leverage AI Remediation - Use IBM BOB to automatically generate fixes for vulnerable cryptography
Continuous Scanning - Re-scan applications after remediation to verify fixes
Maintain Audit Trails - Keep records of all cryptographic changes and remediations
Prioritize High-Risk Findings - Address critical vulnerabilities first
Test Thoroughly - Validate all cryptographic changes in non-production environments
Plan for Post-Quantum - Prepare migration paths to NIST PQC algorithms
Related Capabilities¶
Within Secure:

Non-human Identity - Identity and access management
Other Building Blocks:

Infrastructure as Code - Automated infrastructure provisioning
iPaaS - Integration platform capabilities
Code Modernization - Modernize security middleware
Automated Resilience & Compliance - Ensure cryptographic compliance


Retrieval - Building Blocks¶
Welcome to the Retrieval Building Blocks documentation. These accelerators enable AI applications to access, query, and interact with data through various interfaces and storage mechanisms.

Overview¶
Retrieval capabilities provide the "data access layer" for AI applications, enabling semantic search, NoSQL storage, and efficient federated data retrieval across multiple sources.

Available Building Blocks¶
RAG (Retrieval-Augmented Generation)¶
Complete RAG pipeline with document ingestion, embedding generation, vector storage, and semantic search capabilities.

Key Features:

Document ingestion from IBM Cloud Object Storage
Embedding generation with IBM Watsonx.ai
Vector storage (Milvus or OpenSearch)
Semantic, keyword, and hybrid search
MCP server integration for AI assistants
Bob modes for RAG development guidance
Components:

RAG Accelerator: Complete pipeline with FastAPI REST API
RAG Ingestion MCP Server: Document ingestion for AI assistants
RAG Retrieval MCP Server: Semantic and keyword search
Bob Modes: AI assistant modes for RAG development
Vector Search¶
Vector ingestion, embedding, and retrieval for semantic similarity search in GenAI pipelines.

Key Features:

Document parsing and extraction
Multiple embedding strategies (dense, hybrid, dual)
Flexible chunking strategies
REST API with authentication
Supported Databases:

Milvus: High-performance vector database
OpenSearch: Hybrid vector and keyword search
DataStax Astra DB: Cloud-native vector database
No SQL Database¶
Large-scale NoSQL storage with Cassandra compatibility and optional vector capabilities for AI and application workloads.

Key Features:

Apache Cassandra-based serverless database
Vector collections for AI applications
Data API and CQL support
Scalable and highly available
Zero Copy¶
Federated analytics without copying data. Query data across distributed sources with open lakehouse architecture.

Key Benefits:

Cost Savings: No redundant storage costs
Faster Insights: Avoids ETL delays
Single Source of Truth: Reduces data inconsistencies
Flexibility: Multiple engines access the same data
Governance: Centralized access control
IBM Products:

IBM watsonx.data
IBM Cloud Object Storage (COS)
IBM Db2 Database
Presto Query Engine
Use Cases¶
Common Retrieval Scenarios

RAG Systems: Build complete Retrieval-Augmented Generation pipelines
Question Answering: Intelligent Q&A over document collections
Semantic Search: Find documents based on meaning, not just keywords
Hybrid Search: Combine semantic understanding with keyword precision
Knowledge Management: Create searchable knowledge bases from unstructured data
AI Assistant Integration: Add RAG capabilities via MCP servers
Multi-Cloud Analytics: Query data across AWS, IBM Cloud, and on-premises
Real-Time Insights: Access live data without ETL delays
NoSQL Storage: Scalable storage for AI application data
Resources¶
GitHub Repository
IBM watsonx.data Documentation
IBM watsonx.ai Documentation


RAG (Retrieval-Augmented Generation)¶
Complete RAG pipeline with document ingestion, embedding generation, vector storage, and semantic search capabilities. Supports both Milvus and OpenSearch as vector databases with IBM Watsonx embeddings.

Overview¶
The RAG building block provides a complete end-to-end pipeline for implementing Retrieval-Augmented Generation systems. It handles document processing, embedding generation, vector storage, and semantic search to enable AI applications to access and retrieve relevant information from large document collections.

Key Capabilities:

Document ingestion from IBM Cloud Object Storage (COS)
Embedding generation with IBM Watsonx.ai
Vector storage in Milvus or OpenSearch
Semantic search with vector similarity
Keyword search with BM25 algorithm
Hybrid search combining semantic and keyword approaches
FastAPI-based REST API
Docker deployment ready
MCP server integration for AI assistants
What's Included¶
Assets¶
RAG Accelerator¶
Complete RAG pipeline with document processing, embedding, and querying capabilities.

Features:

Ingest documents from IBM Cloud Object Storage (COS)
Generate embeddings with IBM Watsonx.ai
Store vectors in Milvus or OpenSearch
Perform semantic search with vector similarity
Keyword search with BM25 algorithm
Hybrid search combining semantic and keyword approaches
FastAPI-based REST API
Docker deployment ready
RAG Ingestion MCP Server¶
MCP server for document ingestion from IBM COS.

Features:

Deploy as remote MCP server via SSE transport
Integrate with AI assistants (IBM Bob, Claude, etc.)
Support for multiple document formats
Batch ingestion capabilities
RAG Retrieval MCP Server¶
MCP server for semantic and keyword search.

Features:

Semantic retrieval with Watsonx embeddings
Keyword search with BM25
Hybrid search combining both approaches
Works with both Milvus and OpenSearch backends
Configurable reranking options
Bob Modes¶
Base Modes¶
AI assistant modes specialized for RAG development.

Available Modes:

RAG Builder Mode: Guidance for building RAG pipelines
Data Generator Mode: Help with test data generation
Vector database configuration (Milvus/OpenSearch)
Document processing and chunking strategies
MCP server development assistance
Embedding model selection and optimization
Vector Database Support¶
Milvus¶
High-performance vector database optimized for similarity search.

Features:

High-performance vector similarity search
Scalable distributed architecture
Support for multiple index types (IVF_FLAT, HNSW, etc.)
Rich filtering capabilities
Ideal for large-scale deployments
OpenSearch¶
Combines vector search with full-text search capabilities.

Features:

Combines vector search with full-text search
Built-in BM25 keyword search
Powerful aggregations and analytics
Familiar Elasticsearch-compatible API
Excellent for hybrid search scenarios
Quick Start¶
1. For Complete RAG Pipeline¶
Navigate to assets/rag-accelerator and follow the README:

Configure your vector database (Milvus or OpenSearch)
Set up IBM Watsonx credentials
Deploy via Docker or run locally
2. For MCP Servers¶
Choose from ingestion or retrieval MCP servers in the Assets directory:

Deploy as remote SSE-based MCP servers
Integrate with AI coding assistants
Enable RAG capabilities in your AI workflows
3. For AI Assistance¶
Use the Bob Mode configurations in bob-modes/base-modes with IBM Bob:

Import the RAG Builder or Data Generator modes
Get expert guidance on RAG implementation
Optimize your RAG pipeline design
Use Cases¶
Common RAG Applications

Question Answering: Build intelligent Q&A systems over your documents

Semantic Search: Find relevant information based on meaning, not just keywords

Document Analysis: Extract insights from large document collections

Knowledge Management: Create searchable knowledge bases from unstructured data

AI Assistant Integration: Add RAG capabilities to AI coding assistants via MCP

Hybrid Search: Combine semantic understanding with keyword precision

Architecture¶
Documents in COS
RAG Ingestion
IBM Watsonx.ai
Embeddings
Vector Database
Milvus
OpenSearch
User Query
RAG Retrieval
Search Results
AI Application
IBM Products Used¶
IBM Watsonx.ai: Embedding generation and LLM capabilities
IBM Cloud Object Storage (COS): Document storage and ingestion
Milvus: High-performance vector database (optional)
OpenSearch: Hybrid vector and keyword search (optional)
Resources¶
GitHub Repository - RAG Building Block
RAG Accelerator
RAG Ingestion MCP Server
RAG Retrieval MCP Server
IBM Watsonx.ai Documentation
Milvus Documentation
OpenSearch Documentation


Vector Search Building Block¶
The Vector Search building block provides a modular framework for building GenAI pipelines that combine document parsing and extraction with vector databases for semantic search capabilities.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Vector Search

Overview¶
This building block offers an ingestion API that simplifies the process of chunking, embedding, and storing documents in vector databases. It's designed to save significant development and testing time by providing ready-to-use pipelines with extensible customization options.

Vector Search architecture

IBM Products Used¶
This building block leverages the following IBM products and services:

watsonx.ai: Foundation models and embedding services for document vectorization
watsonx.data: Data lakehouse platform with integrated vector database support
IBM Cloud Object Storage (COS): Scalable object storage for document repositories
Milvus: Open-source vector database for semantic search (integrated with watsonx.data)
Features¶
Ingestion Pipeline: Chunking, merging, and ingestion into vector databases
Embedding Options: Dense, hybrid, or dual embeddings with selectable models
Document Processing: Docling-based parsing with support for HTML, JSON, PDF, Markdown
Flexible Chunking: Multiple chunking strategies (Docling hybrid, Markdown text splitter, recursive)
REST API: Easy-to-use API with authentication
Supported Vector Databases¶
The building block provides integrations with multiple vector database platforms, each optimized for different use cases and deployment scenarios.

Available Integrations

Milvus: High-performance vector database optimized for billion-scale vector search ✅ Available Now
OpenSearch: Enterprise search with hybrid vector and keyword search capabilities 🔄 Planned
DataStax Astra DB: Cloud-native vector database with global distribution 🔄 Planned
Key Capabilities¶
Document Loaders¶
HTML documents
JSON files
PDF documents
Markdown files
Custom loaders
Embedding Models¶
Dense embeddings: Traditional vector representations
Hybrid embeddings: Combination of dense and sparse vectors
Dual embeddings: Separate embeddings for different purposes
Support for HuggingFace, watsonx.ai, and IBM models
Document Processing¶
Docling/Markdown processing
Picture annotation
Table cleanup
Custom processing pipelines
Chunking Strategies¶
Docling hybrid chunker: Intelligent chunking based on document structure
Markdown text splitter: Preserves markdown formatting
Recursive text splitter: Hierarchical text splitting
Deployment Options¶
The Vector Search API can be deployed:

Locally: For development and testing
IBM Code Engine: Serverless container platform
Red Hat OpenShift: Enterprise Kubernetes platform
Docker: Containerized deployment
Getting Started¶
Prerequisites¶
Requirements

watsonx.data environment with Milvus vector database
Python 3.12 installed locally
git installed locally
IBM COS credentials
Vector database credentials
Installation¶
Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data/retrieval/vector-search/
Create a Python virtual environment:


python3 -m venv virtual-env
source virtual-env/bin/activate
pip3 install -r requirements.txt
Configure environment variables:


cp env .env
Update .env with your credentials:

Vector DB credentials: Host, port, username, password
IBM COS credentials: API key, endpoint, service instance ID
REST_API_KEY: Set a unique value for API authentication
Starting the Application¶
Start the application locally:


python3 main.py
Or using Uvicorn:


uvicorn app.main:app --host 127.0.0.1 --port 4050 --reload
Access Swagger UI at: http://127.0.0.1:4050/docs

API Usage¶
Ingestion Endpoint¶
Endpoint: POST /ingest-files

Request Body:


{
    "bucket_name": "<cos-bucket>",
    "collection_name": "<collection-name>",
    "chunk_type": "DOCLING_DOCS"
}
Parameters:

bucket_name: Name of the S3/COS bucket containing documents
collection_name: Target collection to create or upsert into
chunk_type: Chunking strategy (DOCLING_DOCS, MARKDOWN, RECURSIVE)
Headers:


REST_API_KEY: <your-secret>
Content-Type: application/json
Example using Python:


import json, requests

url = "http://127.0.0.1:4050/ingest-files"

payload = json.dumps({
    "bucket_name": "<cos-bucket>",
    "collection_name": "<collection-name>",
    "chunk_type": "DOCLING_DOCS"
})

headers = {
    "REST_API_KEY": "<your-secret>",
    "Content-Type": "application/json"
}

response = requests.post(url, headers=headers, data=payload)
print(response.text)
Use Cases¶
Semantic Search: Find documents based on meaning, not just keywords
RAG Pipelines: Retrieval-augmented generation for LLMs
Knowledge Bases: Build searchable knowledge repositories
Document Discovery: Find similar documents across large collections
Question Answering: Retrieve relevant context for Q&A systems
Customization¶
The API supports extensive customization:

Collection Schema: Configurable via JSON templates
Embedding Models: Choose from multiple providers and models
Document Processing: Custom processing pipelines
Chunking Strategies: Adjust chunk size and overlap
Metadata Extraction: Custom metadata fields
Coming Soon¶
Upcoming Features

.png and .jpg VLM Support
Additional docling processing functions (image annotation, table exports)
Enhanced error logging with structured logs
Performance optimization for large-scale ingestion
Additional vector database integrations
Performance Considerations¶
Optimization Guidelines

Batch Processing: Process multiple documents in parallel
Chunk Size: Balance between context and retrieval precision
Embedding Dimensions: Higher dimensions = more accuracy but slower
Index Configuration: Optimize for your query patterns
Resources¶
GitHub Repository
Milvus Documentation
Team¶
Created and Architected By: Anand Das, Anindya Neogi, Joseph Kim, Shivam Solanki



Milvus Vector Search¶
High-performance vector database optimized for billion-scale vector search with IBM watsonx integration.

Overview¶
Milvus is an open-source vector database built for AI applications, offering high-performance similarity search and analytics for embedding vectors. This building block provides a complete FastAPI service for ingesting documents from IBM Cloud Object Storage (COS) into Milvus with Docling-based parsing and IBM Watsonx embeddings.

IBM Products Used¶
This building block leverages the following IBM products and services:

watsonx.data: Data lakehouse platform with integrated Milvus vector database support
watsonx.ai: Foundation models and embedding services for document vectorization
IBM Cloud Object Storage (COS): Scalable object storage for document repositories
Milvus: Open-source vector database for semantic search (integrated with watsonx.data)
Features¶
Data Ingestion Service¶
FastAPI-based REST API for document ingestion
Docling-based document parsing and processing
IBM Watsonx embedding generation
Automatic vector storage and indexing in Milvus
Interactive Swagger UI for API testing
Document Processing¶
Support for multiple document formats (PDF, HTML, JSON, Markdown)
Intelligent chunking strategies:
DOCLING_DOCS: Structure-aware chunking based on document layout
MARKDOWN: Preserves markdown formatting during chunking
RECURSIVE: Hierarchical text splitting
Metadata extraction and preservation
Vector Operations¶
Automatic collection creation and schema management
Efficient vector upsert operations
Configurable embedding dimensions
Index optimization for fast similarity search
Architecture¶

IBM COS → FastAPI Service → Docling Parser → Watsonx Embeddings → Milvus DB
The service pulls documents from COS, processes them with Docling, generates embeddings using Watsonx, and stores the vectors in Milvus for semantic search.

Getting Started¶
Prerequisites¶
Requirements

watsonx.data environment with Milvus database configured
Setup Guide
Python 3.12 installed locally
git installed locally
Milvus credentials (host, port, username, password)
IBM COS credentials (API key, endpoint, service instance ID)
Installation¶
Clone the repository:


git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data/retrieval/vector-search/milvus/assets/data-ingestion-asset/
Create a Python virtual environment:


python3 -m venv virtual-env
source virtual-env/bin/activate
pip3 install -r requirements.txt
Configure environment variables:


cp .env.example .env
Update .env with your credentials:

Milvus Credentials: - WXD_MILVUS_HOST: Milvus host URL from watsonx.data UI - WXD_MILVUS_PORT: Milvus port from watsonx.data UI - WXD_MILVUS_USER: Set to 'ibmlhapikey' - WXD_MILVUS_PASSWORD: IBM Cloud API Key for Milvus service account

IBM COS Credentials: - IBM_CLOUD_API_KEY: IBM Cloud API Key for COS access - COS_ENDPOINT: Service endpoint URL for your COS instance - COS_SERVICE_INSTANCE_ID: CRN value of COS instance

API Security: - REST_API_KEY: Set a unique value for API authentication

Starting the Application¶
Start the application locally:


python3 main.py
Or using Uvicorn:


uvicorn app.main:app --host 127.0.0.1 --port 4050 --reload
Access Swagger UI at: http://127.0.0.1:4050/docs

API Usage¶
Ingestion Endpoint¶
Endpoint: POST /ingest-files

Request Body:


{
    "bucket_name": "<cos-bucket>",
    "collection_name": "<milvus-collection>",
    "chunk_type": "DOCLING_DOCS"
}
Parameters:

bucket_name: Name of the S3/COS bucket containing documents
collection_name: Target Milvus collection to create or upsert into
chunk_type: Chunking strategy (DOCLING_DOCS, MARKDOWN, RECURSIVE)
Headers:


REST_API_KEY: <your-secret>
Content-Type: application/json
Example using Python:


import json, requests

url = "http://127.0.0.1:4050/ingest-files"

payload = json.dumps({
    "bucket_name": "<cos-bucket>",
    "collection_name": "<milvus-collection>",
    "chunk_type": "DOCLING_DOCS"
})

headers = {
    "REST_API_KEY": "<your-secret>",
    "Content-Type": "application/json"
}

response = requests.post(url, headers=headers, data=payload)
print(response.text)
Testing via Swagger UI¶
Navigate to http://127.0.0.1:4050/docs
Expand POST /ingest-files
Click Try it out
Fill in bucket_name, collection_name, and chunk_type
Click Execute
Verify the 200 response and review ingestion statistics
Use Cases¶
Semantic Search: Find documents based on meaning, not just keywords
RAG Pipelines: Retrieval-augmented generation for LLMs
Knowledge Bases: Build searchable knowledge repositories
Document Discovery: Find similar documents across large collections
Question Answering: Retrieve relevant context for Q&A systems
Content Recommendation: Suggest similar content based on embeddings
Chunking Strategies¶
DOCLING_DOCS¶
Structure-aware chunking based on document layout
Preserves document hierarchy (headings, sections, paragraphs)
Optimal for well-structured documents
Best for maintaining context across document sections
MARKDOWN¶
Preserves markdown formatting during chunking
Respects markdown structure (headers, lists, code blocks)
Ideal for markdown-formatted documentation
Maintains formatting for better readability
RECURSIVE¶
Hierarchical text splitting with configurable chunk size
Splits on multiple separators (paragraphs, sentences, words)
Flexible for various document types
Good for general-purpose chunking
Performance Considerations¶
Optimization Guidelines

Batch Processing: Process multiple documents in parallel for faster ingestion
Chunk Size: Balance between context preservation and retrieval precision
Embedding Dimensions: Higher dimensions provide more accuracy but slower search
Index Type: Choose appropriate index type (IVF_FLAT, HNSW) based on use case
Collection Sharding: Distribute data across multiple shards for scalability
Coming Soon¶
Upcoming Features

.png and .jpg VLM (Vision Language Model) support
Additional Docling processing functions:
Image annotation
Table exports
Enhanced error logging with structured logs
Performance optimization for large-scale ingestion
Resources¶
GitHub Repository
Milvus Documentation
watsonx.data Milvus Setup
Team¶
Created and Architected By: Anand Das, Anindya Neogi, Joseph Kim, Shivam Solanki

OpenSearch Vector Search¶
Enterprise-grade search and analytics engine with vector search capabilities for AI-powered applications.

Overview¶
OpenSearch is an open-source, distributed search and analytics suite derived from Elasticsearch. It provides powerful vector search capabilities through its k-NN (k-Nearest Neighbors) plugin, enabling semantic search and similarity matching for AI/ML applications.

Implementation Status

OpenSearch integration is planned for future releases. This page provides information about OpenSearch capabilities and use cases to help developers understand its potential in the building blocks framework.

Why OpenSearch for Vector Search?¶
OpenSearch combines traditional full-text search with modern vector search capabilities, making it ideal for hybrid search scenarios where you need both keyword matching and semantic similarity.

Key Advantages¶
Hybrid Search: Combine keyword search with vector similarity in a single query
Scalability: Distributed architecture handles billions of vectors
Real-time Indexing: Near real-time updates for dynamic datasets
Rich Ecosystem: Extensive tooling, dashboards, and integrations
Enterprise Features: Security, monitoring, and management capabilities
Core Features¶
Vector Search Capabilities¶
k-NN Search

Approximate nearest neighbor search using HNSW (Hierarchical Navigable Small World) algorithm
Exact k-NN search for smaller datasets
Configurable distance metrics (Euclidean, Cosine, Inner Product)
Support for multiple vector fields per document
Hybrid Search

Combine vector similarity with BM25 text scoring
Weighted scoring between semantic and keyword matches
Filter vectors based on metadata attributes
Boost results based on business logic
Index Management

Automatic index optimization
Index lifecycle management
Snapshot and restore capabilities
Cross-cluster replication
Performance Optimization¶
HNSW Algorithm: Fast approximate nearest neighbor search
Index Segmentation: Distribute vectors across shards
Caching: Query result caching for frequently accessed vectors
Compression: Vector quantization to reduce storage
Use Cases¶
Enterprise Search¶
Semantic Document Search

Find documents based on meaning, not just keywords
Improve search relevance with contextual understanding
Support multi-language search with cross-lingual embeddings
Knowledge Management

Build intelligent knowledge bases
Enable natural language queries over enterprise content
Discover related documents and insights
E-Commerce & Retail¶
Product Discovery

Visual search using image embeddings
"Find similar products" recommendations
Personalized product suggestions based on user behavior
Customer Support

Semantic FAQ search
Automated ticket routing based on content similarity
Knowledge base article recommendations
Media & Content¶
Content Recommendation

Suggest similar articles, videos, or podcasts
Content discovery based on user preferences
Duplicate content detection
Image & Video Search

Search media libraries by visual similarity
Find similar scenes or objects across content
Automated content tagging and categorization
Healthcare & Life Sciences¶
Medical Literature Search

Semantic search across research papers
Find similar patient cases
Drug discovery through molecular similarity
Clinical Decision Support

Match patient symptoms to similar cases
Recommend treatment protocols
Identify relevant clinical trials
Integration with IBM Products¶
IBM watsonx.ai¶
Generate embeddings using IBM foundation models
Leverage watsonx.ai for document understanding
Integrate with RAG pipelines for question answering
IBM Cloud Object Storage¶
Store source documents in COS
Process and index documents from COS buckets
Archive historical data while maintaining search access
IBM watsonx.data¶
Federated queries across OpenSearch and data lakehouse
Unified data governance and access control
Seamless data movement between systems
Comparison with Other Vector Databases¶
Feature	OpenSearch	Milvus	Pinecone
Hybrid Search	✅ Native	⚠️ Limited	❌ No
Full-text Search	✅ Excellent	❌ No	❌ No
Scalability	✅ Billions	✅ Billions	✅ Billions
Open Source	✅ Yes	✅ Yes	❌ No
Managed Service	✅ AWS	⚠️ Limited	✅ Yes
Analytics	✅ Built-in	⚠️ Limited	❌ No
Visualization	✅ Dashboards	❌ No	⚠️ Limited
Best Practices¶
Index Design¶
Optimization Guidelines

Dimension Selection: Balance between accuracy and performance (768-1536 dimensions typical)
Shard Configuration: Distribute vectors across multiple shards for scalability
Replica Strategy: Use replicas for high availability and read performance
Refresh Interval: Adjust based on real-time requirements vs. indexing throughput
Query Optimization¶
Filter First: Apply metadata filters before vector search
Limit Results: Request only needed results (k value)
Use Approximate Search: HNSW for large-scale deployments
Cache Frequently: Cache common queries and embeddings
Monitoring & Maintenance¶
Index Health: Monitor shard status and allocation
Query Performance: Track search latency and throughput
Resource Usage: Monitor CPU, memory, and disk utilization
Index Optimization: Regular force merge for read-heavy workloads
Security & Governance¶
Access Control¶
Role-based access control (RBAC)
Field-level security for sensitive data
Document-level security based on user permissions
Audit logging for compliance
Data Protection¶
Encryption at rest and in transit
Secure inter-node communication
Integration with enterprise identity providers
Data masking for PII protection
Performance Characteristics¶
Scalability¶
Horizontal Scaling: Add nodes to increase capacity
Vertical Scaling: Increase resources per node
Index Sharding: Distribute data across cluster
Query Distribution: Parallel query execution
Latency¶
Approximate k-NN: Sub-100ms for millions of vectors
Exact k-NN: Suitable for smaller datasets (<100K vectors)
Hybrid Queries: Slightly higher latency than pure vector search
Caching: Significant improvement for repeated queries
Future Integration Plans¶
Roadmap

The OpenSearch integration for the building blocks framework will include:

Ingestion API: FastAPI service for document processing and indexing
Hybrid Search: Combined keyword and semantic search capabilities
IBM watsonx Integration: Native embedding generation using watsonx.ai
Monitoring Dashboard: Real-time metrics and performance tracking
Bob Mode Support: AI-assisted OpenSearch configuration and optimization
Resources¶
Documentation¶
OpenSearch Official Documentation
k-NN Plugin Guide
OpenSearch on AWS
Learning Resources¶
OpenSearch Vector Search Tutorial
Hybrid Search Best Practices
Performance Tuning Guide

DataStax Astra DB Vector Search¶
Cloud-native vector database built on Apache Cassandra with serverless scalability and global distribution.

Overview¶
DataStax Astra DB is a cloud-native database-as-a-service built on Apache Cassandra, offering vector search capabilities for AI applications. It combines the proven scalability and reliability of Cassandra with modern vector search features, making it ideal for production AI workloads that require global distribution and high availability.

Implementation Status

DataStax Astra DB integration is planned for future releases. This page provides information about Astra DB capabilities and use cases to help developers understand its potential in the building blocks framework.

Why DataStax Astra DB for Vector Search?¶
Astra DB brings enterprise-grade reliability and global scale to vector search, making it suitable for mission-critical AI applications that need to serve users worldwide with low latency.

Key Advantages¶
Serverless Architecture: Auto-scaling without infrastructure management
Global Distribution: Multi-region deployment with active-active replication
High Availability: 99.99% uptime SLA with automatic failover
Cassandra Foundation: Battle-tested distributed database technology
Unified Platform: Combine vector search with traditional database operations
Core Features¶
Vector Search Capabilities¶
Vector Similarity Search

Approximate nearest neighbor (ANN) search
Support for multiple distance metrics (Cosine, Euclidean, Dot Product)
Configurable accuracy vs. performance trade-offs
Real-time vector indexing and updates
Hybrid Data Model

Store vectors alongside structured data
Query vectors with metadata filters
Combine vector similarity with traditional queries
Support for multiple vector columns per table
Scalability

Horizontal scaling across nodes
Automatic data distribution and replication
Linear performance scaling with cluster size
Support for billions of vectors
Database Features¶
Multi-Model Support

Document API for JSON data
REST API for easy integration
GraphQL API for flexible queries
CQL (Cassandra Query Language) for advanced operations
Data Management

Automatic data replication across regions
Configurable consistency levels
Time-to-live (TTL) for automatic data expiration
Change data capture (CDC) for real-time streaming
Security & Compliance

Encryption at rest and in transit
Role-based access control (RBAC)
SOC 2, HIPAA, and GDPR compliance
Private endpoints and VPC peering
Use Cases¶
Global Applications¶
Multi-Region Deployment

Serve users from nearest data center
Active-active replication for write availability
Disaster recovery with automatic failover
Compliance with data residency requirements
Low-Latency Search

Sub-100ms query latency globally
Edge caching for frequently accessed vectors
Optimized for read-heavy workloads
Predictable performance at scale
Enterprise AI Applications¶
Recommendation Systems

Real-time product recommendations
Personalized content delivery
User behavior analysis
A/B testing with vector embeddings
Fraud Detection

Anomaly detection using vector similarity
Real-time transaction analysis
Pattern recognition across user behavior
Historical fraud pattern matching
Customer 360

Unified customer profiles with vector embeddings
Similar customer identification
Churn prediction and prevention
Personalized marketing campaigns
Content & Media¶
Content Discovery

Semantic search across media libraries
Similar content recommendations
Automated content tagging
Duplicate content detection
Digital Asset Management

Image and video similarity search
Brand asset organization
Rights management with metadata
Multi-modal search (text + image)
Healthcare & Life Sciences¶
Patient Matching

Find similar patient cases
Clinical trial matching
Treatment protocol recommendations
Medical literature search
Drug Discovery

Molecular similarity search
Compound screening
Target identification
Literature mining
Integration with IBM Products¶
IBM watsonx.ai¶
Generate embeddings using IBM foundation models
Integrate with watsonx.ai for document processing
Support for RAG (Retrieval-Augmented Generation) pipelines
Real-time embedding updates
IBM Cloud Object Storage¶
Store source documents in COS
Process and vectorize documents from COS
Archive historical data with metadata
Seamless data pipeline integration
IBM watsonx.data¶
Federated queries across Astra DB and lakehouse
Unified data governance
Cross-platform analytics
Data movement and synchronization
Comparison with Other Vector Databases¶
Feature	Astra DB	Milvus	OpenSearch
Global Distribution	✅ Native	❌ No	⚠️ Limited
Serverless	✅ Yes	❌ No	⚠️ AWS Only
Multi-Model	✅ Yes	❌ No	⚠️ Limited
High Availability	✅ 99.99%	⚠️ Manual	✅ Yes
Managed Service	✅ Fully	⚠️ Limited	✅ AWS
Open Source	⚠️ Cassandra	✅ Yes	✅ Yes
Consistency	✅ Tunable	⚠️ Eventual	✅ Strong
Best Practices¶
Data Modeling¶
Design Guidelines

Partition Key Design: Distribute data evenly across nodes
Vector Dimensions: Balance between accuracy and storage (384-1536 typical)
Denormalization: Store related data together for query efficiency
TTL Strategy: Use time-to-live for temporary data
Performance Optimization¶
Replication Factor: Balance between availability and cost
Consistency Level: Choose based on application requirements
Batch Operations: Use batch inserts for bulk data loading
Connection Pooling: Reuse connections for better performance
Scalability Planning¶
Capacity Planning: Monitor storage and throughput metrics
Auto-scaling: Configure thresholds for automatic scaling
Region Selection: Deploy in regions close to users
Data Distribution: Ensure even data distribution across partitions
Security & Governance¶
Access Control¶
Role-based access control (RBAC)
Fine-grained permissions per keyspace/table
API token management
IP allowlisting and VPC peering
Compliance¶
SOC 2 Type II certified
HIPAA compliant
GDPR compliant
ISO 27001 certified
Data Protection¶
Encryption at rest (AES-256)
Encryption in transit (TLS 1.2+)
Automated backups with point-in-time recovery
Data masking for sensitive information
Performance Characteristics¶
Scalability¶
Horizontal Scaling: Add nodes without downtime
Linear Performance: Performance scales with cluster size
Multi-Region: Active-active replication across regions
Serverless: Automatic scaling based on workload
Latency¶
Single-Region: Sub-10ms for local queries
Multi-Region: Sub-100ms for global queries
Vector Search: Optimized ANN algorithms
Caching: Built-in caching for hot data
Throughput¶
Writes: Millions of writes per second
Reads: Optimized for read-heavy workloads
Concurrent Users: Support for thousands of concurrent connections
Batch Operations: Efficient bulk data operations
Cost Optimization¶
Serverless Pricing¶
Pay only for storage and operations used
No idle capacity costs
Automatic scaling reduces over-provisioning
Predictable pricing model
Storage Optimization¶
Compression for reduced storage costs
TTL for automatic data expiration
Tiered storage for historical data
Efficient vector storage formats
Future Integration Plans¶
Roadmap

The DataStax Astra DB integration for the building blocks framework will include:

Ingestion API: FastAPI service for document processing and vectorization
Global Deployment: Multi-region configuration templates
IBM watsonx Integration: Native embedding generation using watsonx.ai
Monitoring Dashboard: Real-time metrics and performance tracking
Bob Mode Support: AI-assisted Astra DB configuration and optimization
Migration Tools: Data migration from other vector databases
Resources¶
Documentation¶
DataStax Astra DB Documentation
Vector Search Guide
API Reference
Learning Resources¶
Astra DB Quickstart
Vector Search Tutorial
Best Practices Guide
Community¶
DataStax Community
DataStax Academy
GitHub Examples


No SQL Database¶
Large-scale NoSQL storage with Cassandra compatibility and optional vector capabilities for AI and application workloads.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - NoSQL Database

Overview¶
The No SQL Database building block provides large-scale NoSQL storage with Cassandra compatibility and optional vector capabilities for AI and application workloads. It offers a serverless, cloud-native database solution that scales automatically based on demand.

IBM Products Used¶
This building block leverages the following products and services:

DataStax Astra DB: Cloud-native, serverless database built on Apache Cassandra
Apache Cassandra: Distributed NoSQL database for high availability and scalability
IBM watsonx.data: Integration for unified data access
Features¶
Cassandra-Based Architecture¶
Distributed Architecture: Multi-region, multi-cloud deployment
High Availability: No single point of failure
Linear Scalability: Scale horizontally by adding nodes
Tunable Consistency: Balance between consistency and availability
Vector Capabilities¶
Vector Collections: Store and query vector embeddings
Similarity Search: Find similar items based on vector distance
AI Integration: Seamless integration with AI/ML workflows
Hybrid Search: Combine traditional and vector search
Data API & CQL Support¶
REST API: HTTP-based Data API for easy integration
CQL (Cassandra Query Language): SQL-like query language
Multiple Client Libraries: Support for various programming languages
GraphQL Support: Modern API query language
Use Cases¶
AI/ML Applications: Store embeddings and perform similarity search
IoT Data Storage: Handle high-volume time-series data
User Profile Management: Store and retrieve user data at scale
Product Catalogs: Manage large product inventories
Real-Time Analytics: Process and analyze streaming data
Getting Started¶
Prerequisites¶
Requirements

DataStax Astra DB account (free tier available)
Application credentials (token or username/password)
Client library for your programming language
Network connectivity to Astra DB endpoints
Basic Setup¶
Create an Astra DB database
Sign up at astra.datastax.com
Create a new database
Generate application token

Connect to your database


from cassandra.cluster import Cluster
from cassandra.auth import PlainTextAuthProvider

cloud_config = {
    'secure_connect_bundle': '/path/to/secure-connect-database.zip'
}

auth_provider = PlainTextAuthProvider('token', 'your-token')
cluster = Cluster(cloud=cloud_config, auth_provider=auth_provider)
session = cluster.connect()
Create a keyspace and table


CREATE KEYSPACE IF NOT EXISTS my_keyspace 
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

CREATE TABLE my_keyspace.my_table (
    id UUID PRIMARY KEY,
    name TEXT,
    data TEXT
);
Architecture Pattern¶
Astra DB architecture overview

Distributed Storage
Node 1
Node 2
Node 3
DataStax Astra DB
Data API
CQL Interface
Vector Search
Applications
Web App
Mobile App
AI Service
Vector Search Example¶

# Create a vector-enabled collection
from astrapy.db import AstraDB

db = AstraDB(
    token="your-token",
    api_endpoint="your-endpoint"
)

collection = db.create_collection(
    collection_name="vector_collection",
    dimension=1536,  # OpenAI embedding dimension
    metric="cosine"
)

# Insert vectors
collection.insert_one({
    "_id": "doc1",
    "text": "Sample document",
    "$vector": [0.1, 0.2, 0.3, ...]  # 1536-dimensional vector
})

# Perform similarity search
results = collection.find(
    sort={"$vector": [0.1, 0.2, 0.3, ...]},
    limit=5
)
Best Practices¶
NoSQL Best Practices

Data Modeling: Design tables based on query patterns, not normalization
Partition Keys: Choose partition keys that distribute data evenly
Consistency Levels: Select appropriate consistency for your use case
Batch Operations: Use batch statements for multiple writes
Monitoring: Track performance metrics and query patterns
Indexing: Use secondary indexes judiciously
Performance Considerations¶
Partition Size: Keep partitions under 100MB for optimal performance
Query Patterns: Design tables to support your most common queries
Replication Factor: Balance between availability and write performance
Compaction: Configure compaction strategies based on workload
Connection Pooling: Reuse connections for better performance
Coming Soon¶
Upcoming Features

Detailed implementation guides
Sample applications with vector search
Integration patterns with watsonx.ai
Performance tuning guidelines
Migration guides from other databases
Resources¶
DataStax Astra DB Documentation
Apache Cassandra Documentation
Vector Search Guide
GitHub Repository

Zero Copy¶
Federated analytics without copying data. Query data across distributed sources with open lakehouse architecture.

GitHub Repository

The complete source code and examples are available in the GitHub repository:

Building Blocks - Zero Copy

What is Zero-Copy Lakehouse?¶
A Zero-Copy Lakehouse is a data architecture approach where multiple analytics, AI, and ML tools can access and process the same underlying data without duplicating or moving it across systems.

Instead of copying data between warehouses, lakes, and ML pipelines, a zero-copy approach enables shared access with governance and performance optimizations.

Why It Matters¶
Traditional setups involve ETL (Extract, Transform, Load) pipelines that duplicate data into multiple systems, leading to:

Higher storage costs
Governance risks
Data inconsistencies
Processing delays
Zero-copy lakehouse eliminates data silos by providing a single source of truth for BI, AI, ML, and analytics workloads.

Benefits¶
Key Advantages

Cost Savings: No redundant storage costs
Faster Insights: Avoids ETL delays
Single Source of Truth: Reduces risk of inconsistent data
Flexibility: Multiple engines/tools access the same data
Governance: One layer controls access everywhere
IBM watsonx.data & Zero Copy¶
In the IBM watsonx.data lakehouse:

Built on open table formats (Iceberg/Delta)
Provides federated query capability (query S3, Db2, Cloud Object Storage, external warehouses, all in place)
Ensures zero-copy data access with no need to ETL into another system
Architecture¶
IBM watsonx.data
Presto Engine
IBM Cloud Object Storage (COS)
cos_catalog
Amazon S3
s3_catalog
DB2 OLTP
(db2_catalog)
Federated SQL Queries
Data Scientists / Analysts
Watsonx.data Setup Automation¶
The building block provides a Python script (watsonxdata_setup.py) that automates the setup of IBM watsonx.data resources using official APIs.

What Gets Configured¶
IBM Cloud Object Storage (COS) bucket
Amazon S3 bucket
DB2 Database connection (SaaS on IBM Cloud)
Presto engine catalog associations
Schemas for COS and S3
Prerequisites¶
Requirements

Access to:
IBM Cloud Account and AWS Account
watsonx.data SaaS instance on IBM Cloud
DB2 Database SaaS instance on IBM Cloud
AWS S3 - Simple Cloud Storage

Python 3.12+ installed locally

Install dependencies:


pip install requests
Set your IBM Cloud API key:


export IBM_API_KEY="your-ibm-cloud-api-key"
Getting Started¶
Step 1: Clone the Repository¶

git clone https://github.com/ibm-self-serve-assets/building-blocks.git
cd building-blocks/data/retrieval/zero-copy/assets/setup-lakehouse
Step 2: Configure Settings¶
Edit config.json with your environment details:

Configuration	Description	Example
region	IBM Cloud region	us-south
auth_instance_id	watsonx.data deployment ID	crn:v1:bluemix:public:lakehouse:...
COS Configuration		
bucket_display_name	Display name for COS bucket	cos_bucket
bucket_details.bucket_name	COS bucket name	watsonxdata-demo
bucket_details.endpoint	COS endpoint	https://s3.us-south.cloud-object-storage.appdomain.cloud
associated_catalog.catalog_name	Catalog name	cos_catalog
S3 Configuration		
bucket_display_name	Display name for S3 bucket	amazon_S3
bucket_details.bucket_name	AWS S3 bucket name	watsonxdata
bucket_details.endpoint	S3 endpoint	https://s3.us-east-2.amazonaws.com
associated_catalog.catalog_name	Catalog name	s3_catalog
DB2 Configuration		
database_name	Database name	bludb
hostname	DB2 hostname	87612426-7efe-...db2.ibmappdomain.cloud
port	DB2 port	31687
catalog_name	Catalog name	db2_catalog
Step 3: Run Setup Automation¶

python watsonxdata_setup.py
This will:

Authenticate with IBM Cloud IAM
Create a project
Create a catalog
Register storage buckets
Configure database connections
Demo: Zero-Copy Lakehouse in Action¶
Load Data into Storage Sources¶
Load Data into Amazon S3¶

# Create a folder named 'account' in S3
aws s3 cp account.csv s3://<your-s3-bucket-name>/account/
Load Data into IBM COS¶

# Create a bucket and folder 'customer' in COS, then upload
ibmcloud cos upload --bucket <your-cos-bucket-name> --key customer/customer.csv --file customer.csv
Load Data into Db2¶

CREATE TABLE customer_info.customer_summary (
  customer_id VARCHAR(50),
  total_spend DECIMAL(10,2),
  last_purchase_date DATE
);

IMPORT FROM customer_summary.csv OF DEL
INSERT INTO customer_info.customer_summary;
Define External Tables in watsonx.data¶
Login to the watsonx.data console, go to the Query Workspace, and run:

Create COS Table¶

CREATE TABLE "cos_catalog"."customer"."customer" (
  customer_id VARCHAR,
  customer_name VARCHAR,
  region VARCHAR
)
WITH (
  format = 'CSV',
  external_location = 's3a://watsonxdata-demo/customer/'
);
Create S3 Table¶

CREATE TABLE "s3_catalog"."account"."account" (
  account_id VARCHAR,
  balance VARCHAR,
  customer_id VARCHAR
)
WITH (
  format = 'CSV',
  external_location = 's3a://watsonxdata/account/'
);
Query Across All Three Data Sources¶

SELECT *
FROM "s3_catalog"."account"."account" a
JOIN "cos_catalog"."customer"."customer" c ON a.customer_id = c.customer_id
JOIN "db2_catalog"."customer_info"."customer_summary" cs ON cs.customer_id = a.customer_id;
Zero-Copy in Action

This query demonstrates accessing S3, COS, and Db2 data directly without duplication, enabling Zero-Copy Lakehouse insights.

Use Cases¶
Multi-Cloud Analytics: Query data across AWS, IBM Cloud, and on-premises databases
Cost Optimization: Eliminate redundant data copies and storage costs
Real-Time Insights: Access live data without ETL delays
Data Governance: Centralized access control and compliance
AI/ML Pipelines: Direct access to training data without movement
Best Practices¶
Implementation Guidelines

Keep your API keys secure and never commit them to git
Ensure your S3/COS buckets and Db2 tables exist before running the demo
For larger workloads, consider optimizing with Iceberg/Delta formats
Use appropriate access controls and encryption for sensitive data
Monitor query performance and optimize catalog configurations
IBM Products Used¶
This building block leverages the following IBM products and services:

IBM watsonx.data¶
Open, hybrid, and governed data store built on an open lakehouse architecture.

Purpose: Federated query engine (Presto) for zero-copy data access across multiple sources
Documentation: IBM watsonx.data Documentation
Getting Started: watsonx.data Getting Started
Provisioning: Provisioning watsonx.data
IBM Cloud Object Storage (COS)¶
Scalable, secure object storage for unstructured data with S3-compatible API.

Purpose: Data lake storage with Iceberg/Delta table formats
Documentation: IBM COS Documentation
Integration: COS with watsonx.data
Getting Started: COS Getting Started
IBM Db2 Database¶
Enterprise-grade relational database for OLTP workloads.

Purpose: Operational database for real-time transactional data
Documentation: IBM Db2 Documentation
SaaS Offering: Db2 on Cloud
Provisioning: Provisioning Db2
Amazon S3¶
Cloud object storage service (external integration).

Purpose: Multi-cloud data access without data movement
Documentation: AWS S3 Documentation
Integration: S3 with watsonx.data
Presto Query Engine¶
Distributed SQL query engine for big data analytics (included in watsonx.data).

Purpose: Federated query execution across multiple data sources
Documentation: Presto Documentation
watsonx.data Integration: Presto in watsonx.data
Resources¶
GitHub Repository
IBM watsonx.data Documentation
Adding Storage and Catalog Pair
Adding Database and Catalog Pair