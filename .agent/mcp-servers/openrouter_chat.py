from mcp.server.fastmcp import FastMCP
import httpx
import os
import json
from typing import Optional

# Initialize FastMCP
mcp = FastMCP("OpenRouter Chat")

OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY", "sk-or-your-key-here")

async def call_openrouter(model: str, prompt: str) -> str:
    url = "https://openrouter.ai/api/v1/chat/completions"
    headers = {
        "Authorization": f"Bearer {OPENROUTER_API_KEY}",
        "Content-Type": "application/json",
    }
    data = {
        "model": model,
        "messages": [{"role": "user", "content": prompt}]
    }
    
    async with httpx.AsyncClient() as client:
        try:
            response = await client.post(url, headers=headers, json=data, timeout=60.0)
            response.raise_for_status()
            result = response.json()
            return result["choices"][0]["message"]["content"]
        except Exception as e:
            return f"ERROR: Failed to call OpenRouter: {str(e)}"

@mcp.tool()
async def ask_deepseek(prompt: str) -> str:
    """Ask DeepSeek-R1 for complex logic, math, or C# tasks."""
    return await call_openrouter("deepseek/deepseek-r1", prompt)

@mcp.tool()
async def ask_qwen(prompt: str) -> str:
    """Ask Qwen-2.5-Coder for architectural and project-wide tasks."""
    return await call_openrouter("qwen/qwen-2.5-72b-instruct", prompt)

@mcp.tool()
async def ask_llama(prompt: str) -> str:
    """Ask Llama-3.3-70B for code auditing and dialogue."""
    return await call_openrouter("meta-llama/llama-3.3-70b-instruct", prompt)

if __name__ == "__main__":
    mcp.run()
