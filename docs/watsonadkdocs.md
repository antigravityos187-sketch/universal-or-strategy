Get Started
Getting started with the ADK
The Agent Development Kit (ADK) gives you a set of developer-focused tools to build, test, and manage agents in watsonx Orchestrate. With the ADK, you take full control of agent design using a lightweight framework and a simple CLI.
Define agents in clear YAML or JSON files, create custom Python tools, and manage the entire agent lifecycle with just a few commands.
This guide walks you through installing the ADK, setting up your local development environment, and deploying your first agent to a watsonx Orchestrate SaaS instance. Start building flexible, reusable AI agents right away.
​
Before you begin
​
Required software
Make sure you have Python 3.11 or later installed on your system. To check your current Python version, open a terminal and run:
python --version
If you have an older version of Python, you have a few options to install a newer Python version:
Windows

Linux

macOS

​
watsonx Orchestrate account
To use the ADK, you must connect it to an existing watsonx Orchestrate environment. If you don’t have a watsonx Orchestrate account, you can sign up for a free 30-day trial.
If you already have a watsonx Orchestrate account, you can use your existing account to provide an environment to use with the ADK.
​
Setting up and installing the ADK
1
Installing the ADK

Use the following command to install the ADK with pip:
BASH
pip install --upgrade ibm-watsonx-orchestrate
You can optionally use a virtual environment (venv) or a version manager like uv to control the packages you install. This setup makes it easier to share and distribute your agents and tools with others.
To learn how to install the ADK in these situations, see the following steps:
System installation
Virtual environments
uv
Install the ADK on your system with the following command:
pip install  --upgrade ibm-watsonx-orchestrate
2
Configure your environment in the ADK

Configure your watsonx Orchestrate environment in the ADK. Use this environment to create your agents.
In this step, you need access to specific credentials for your environment. If you don’t know the type of environment you have, see Logging in to IBM watsonx Orchestrate.
IBM Cloud

AWS

On-premises

To add your environment, run the following.
BASH
    orchestrate env add -n <environment-name> -u <service-instance-url>
Note:
You can set any name you prefer for the environment.
In the rare case when the watsonx Orchestrate ADK does not automatically infer the correct authentication type for your url please add the following to the orchestrate env add command:
IBM Cloud: --type ibm_iam
AWS: --type mcsp
On-premises: --type cpd
3
Activate your environment

Run the following command to activate the environment you created:
orchestrate env activate <environment-name>
Environments on AWS GovCloud

If you need to change your environment, see Configuring your environments.
Note: You can also activate a local development environment. This environment is provided by the watsonx Orchestrate Developer Edition, a stripped-down version of watsonx Orchestrate that runs under a Docker container to be used as a development server. To learn more about it, see watsonx Orchestrate Developer Edition.
​
Creating your first agent
With your ADK connected to your watsonx Orchestrate instance, you’re ready to test your setup by publishing a simple agent.
In watsonx Orchestrate, you define agents using YAML or JSON files. The agent specification file includes key details like the agent’s name, kind, instructions for the LLM, and the tools it can use.
When you build agents with the ADK, you write this specification yourself. This approach gives you full control over your agent’s behavior and capabilities. You can create advanced agents with custom tools, collaborators, or integrations. For now, start with a minimal Hello World agent to verify your setup.
For internal GovCloud environments (staging, pre-prod), specify the IAM URL: ./fedramp_activate fedramp --api-key <your_api_key> --iam-url dai.prep.ibmforusgov.com
1
Starting your ADK project

As a Python project, it’s a good practice to create a folder that stores your agents, tools, and other resources.
Example of folder structure:
.
└── adk-project/
    ├── agents/
    ├── tools/
    ├── knowledge/
    ├── flows/
    └── ...

macOS & Linux

Windows
mkdir -p adk-project/{agents,tools,knowledge,flows}
2
Creating the agent specification

Start by creating the YAML file for the agent and name the file as hello-world-agent.yaml. Open the created file with the text editor of your choice, then copy and paste the following code.
hello-world-agent.yaml
spec_version: v1 
kind: native 
name: Hello_World_Agent 
description: A simple Hello World agent 
instructions: "You are a test agent created for a tutorial on how to get started with watsonx Orchestrate ADK. When the user asks 'who are you', respond with: I'm the Hello World Agent. Congratulations on completing the Getting Started with watsonx Orchestrate ADK tutorial!"
llm: groq/openai/gpt-oss-120b
style: default 
collaborators: [] 
tools: [] 
See all 9 lines
And save the file in the agents folder.
3
Import the agent

Open your terminal and navigate to the folder where you created the YAML file. Then run the following command to publish your agent:
orchestrate agents import -f hello-world-agent.yaml 
If the import is successful, you get the following confirmation message:
[INFO] - Agent 'Hello World Agent' imported successfully
4
Open the Agent Builder

Log in to your watsonx Orchestrate instance and open the Agent Builder:
Open the Agent Builder
5
Select the Hello World agent

In the Build agents and tools page, select the Hello_World_Agent.
Select the agent
6
Test your agent

In this page, you can customize your agent’s behavior. Use the test chat to test your agent.
Test your chat
​
What’s next?
You have now created your first agent with the ADK and successfully imported it into your watsonx Orchestrate instance.
To extend your agent’s behavior, you can use tools, knowledge bases and agentic workflows. To connect to services, you must create Connections to safely store credentials and sensitive data. You can also change the agent’s large language model and style by changing the agent specification, and you can test and evaluate the agent with the Evaluation Framework.
See the following cards to learn more about what you can do with the ADK:
Agents
Learn more about what you can do with agents, how you can change your agent style and how to customize your agent’s large language model.
Environments
Learn how to change your watsonx Orchestrate environments to import your agents developed with the ADK.
Tools
Learn more about how to use tools to extend your agent’s capabilities and how to integrate with external services.
Connections
Learn more about how to safely store credentials and connect to external services.
Knowledge Base
Learn how to use knowledge bases to provide more accurate responses and improve your agent’s knowledge in specific situations.
Evaluation Framework
Assess your agent’s behavior by comparing simulated agent interactions with a predefined set of reference data.
What is watsonx Orchestrate ADK
Previous
Tutorial: Creating your first agent with the Agent builder
