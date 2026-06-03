#!/usr/bin/env python3
"""
Jane Street Documentation Extraction Script
V12 Universal OR Strategy - Jane Street Intel Pipeline
Last Updated: 2026-06-03

Extracts README, DESIGN docs, comments, and commit messages from Jane Street repos.
"""

import json
import os
import re
import subprocess
from pathlib import Path
from typing import Dict, List, Optional
from datetime import datetime

# Configuration
REPOS_DIR = Path.home() / ".jane-street"
STATUS_FILE = REPOS_DIR / ".sync-status.json"


def log(message: str, level: str = "INFO"):
    """Log message with timestamp."""
    timestamp = datetime.now().strftime("%H:%M:%S")
    print(f"[{timestamp}] [{level}] {message}")


def load_sync_status() -> Dict:
    """Load sync status from JSON file."""
    if not STATUS_FILE.exists():
        log("No sync status file found", "ERROR")
        return {"repos": {}}
    
    with open(STATUS_FILE, 'r') as f:
        return json.load(f)


def extract_readme(repo_path: Path) -> Optional[str]:
    """Extract README.md content."""
    readme_files = ["README.md", "README.org", "README.txt", "README"]
    
    for readme_name in readme_files:
        readme_path = repo_path / readme_name
        if readme_path.exists():
            try:
                with open(readme_path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                    log(f"  Extracted {readme_name} ({len(content)} chars)", "DEBUG")
                    return content
            except Exception as e:
                log(f"  Failed to read {readme_name}: {e}", "WARN")
    
    return None


def extract_design_doc(repo_path: Path) -> Optional[str]:
    """Extract DESIGN.md or similar design documentation."""
    design_files = ["DESIGN.md", "DESIGN.org", "ARCHITECTURE.md", "docs/DESIGN.md"]
    
    for design_name in design_files:
        design_path = repo_path / design_name
        if design_path.exists():
            try:
                with open(design_path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                    log(f"  Extracted {design_name} ({len(content)} chars)", "DEBUG")
                    return content
            except Exception as e:
                log(f"  Failed to read {design_name}: {e}", "WARN")
    
    return None


def extract_ocaml_comments(repo_path: Path, max_files: int = 50) -> List[str]:
    """Extract top-level comments from OCaml files (.ml, .mli)."""
    comments = []
    ocaml_files = list(repo_path.glob("**/*.ml")) + list(repo_path.glob("**/*.mli"))
    
    # Limit to avoid overwhelming output
    ocaml_files = ocaml_files[:max_files]
    
    for file_path in ocaml_files:
        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                
                # Extract OCaml comments: (* ... *)
                comment_pattern = r'\(\*\s*(.*?)\s*\*\)'
                matches = re.findall(comment_pattern, content, re.DOTALL)
                
                for match in matches:
                    # Only keep substantial comments (>50 chars)
                    if len(match.strip()) > 50:
                        comments.append({
                            "file": str(file_path.relative_to(repo_path)),
                            "comment": match.strip()[:500]  # Truncate to 500 chars
                        })
        except Exception as e:
            log(f"  Failed to parse {file_path.name}: {e}", "DEBUG")
    
    log(f"  Extracted {len(comments)} OCaml comments", "DEBUG")
    return comments


def extract_commit_messages(repo_path: Path, max_commits: int = 100) -> List[Dict]:
    """Extract recent commit messages."""
    try:
        # Get last N commits with format: hash|author|date|message
        cmd = [
            "git", "-C", str(repo_path),
            "log", f"-{max_commits}",
            "--pretty=format:%H|%an|%ai|%s"
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        
        if result.returncode != 0:
            log(f"  Git log failed: {result.stderr}", "WARN")
            return []
        
        commits = []
        for line in result.stdout.strip().split('\n'):
            if not line:
                continue
            
            parts = line.split('|', 3)
            if len(parts) == 4:
                commits.append({
                    "hash": parts[0][:8],  # Short hash
                    "author": parts[1],
                    "date": parts[2],
                    "message": parts[3]
                })
        
        log(f"  Extracted {len(commits)} commit messages", "DEBUG")
        return commits
    
    except subprocess.TimeoutExpired:
        log(f"  Git log timed out", "WARN")
        return []
    except Exception as e:
        log(f"  Failed to extract commits: {e}", "WARN")
        return []


def extract_repo_metadata(repo_path: Path) -> Dict:
    """Extract repository metadata (languages, file counts, etc.)."""
    metadata = {
        "total_files": 0,
        "ocaml_files": 0,
        "markdown_files": 0,
        "size_kb": 0.0
    }
    
    try:
        # Count files by type
        all_files = list(repo_path.rglob("*"))
        metadata["total_files"] = len([f for f in all_files if f.is_file()])
        metadata["ocaml_files"] = len(list(repo_path.glob("**/*.ml")) + list(repo_path.glob("**/*.mli")))
        metadata["markdown_files"] = len(list(repo_path.glob("**/*.md")))
        
        # Calculate total size
        total_size = sum(f.stat().st_size for f in all_files if f.is_file())
        metadata["size_kb"] = round(total_size / 1024, 2)
        
    except Exception as e:
        log(f"  Failed to extract metadata: {e}", "WARN")
    
    return metadata


def extract_repo_docs(repo_name: str, repo_path: Path) -> Dict:
    """Extract all documentation from a single repository."""
    log(f"Extracting docs from {repo_name}...", "INFO")
    
    docs = {
        "repo": repo_name,
        "extracted_at": datetime.now().isoformat(),
        "readme": None,
        "design": None,
        "comments": [],
        "commits": [],
        "metadata": {}
    }
    
    # Extract README
    docs["readme"] = extract_readme(repo_path)
    
    # Extract DESIGN doc
    docs["design"] = extract_design_doc(repo_path)
    
    # Extract OCaml comments
    docs["comments"] = extract_ocaml_comments(repo_path, max_files=50)
    
    # Extract commit messages
    docs["commits"] = extract_commit_messages(repo_path, max_commits=100)
    
    # Extract metadata
    docs["metadata"] = extract_repo_metadata(repo_path)
    
    # Save to JSON
    output_file = repo_path / "extracted-docs.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(docs, f, indent=2, ensure_ascii=False)
    
    log(f"  Saved to {output_file}", "INFO")
    
    return docs


def main():
    """Main extraction workflow."""
    log("=== Jane Street Documentation Extraction ===", "INFO")
    log(f"Repos directory: {REPOS_DIR}", "INFO")
    
    # Load sync status
    sync_status = load_sync_status()
    
    if not sync_status.get("repos"):
        log("No repos found in sync status. Run sync first.", "ERROR")
        return 1
    
    # Filter to indexed repos only
    indexed_repos = [
        name for name, status in sync_status["repos"].items()
        if status.get("status") == "indexed"
    ]
    
    if not indexed_repos:
        log("No indexed repos found. Run sync first.", "ERROR")
        return 1
    
    log(f"Found {len(indexed_repos)} indexed repos", "INFO")
    
    # Extract docs from each repo
    extracted_count = 0
    failed_count = 0
    
    for repo_name in indexed_repos:
        repo_path = REPOS_DIR / repo_name
        
        if not repo_path.exists():
            log(f"Repo path not found: {repo_path}", "WARN")
            failed_count += 1
            continue
        
        try:
            extract_repo_docs(repo_name, repo_path)
            extracted_count += 1
        except Exception as e:
            log(f"Failed to extract {repo_name}: {e}", "ERROR")
            failed_count += 1
    
    # Summary
    log("", "INFO")
    log("=== Extraction Summary ===", "INFO")
    log(f"Total repos: {len(indexed_repos)}", "INFO")
    log(f"Extracted: {extracted_count}", "INFO")
    log(f"Failed: {failed_count}", "INFO")
    
    if failed_count > 0:
        log("Some extractions failed. Check logs above.", "WARN")
        return 1
    
    log("Extraction complete!", "INFO")
    return 0


if __name__ == "__main__":
    exit(main())

# Made with Bob
