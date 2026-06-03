#!/usr/bin/env python3
"""
Jane Street Intel Firestore Upload Script
V12 Universal OR Strategy - Jane Street Intel Pipeline
Last Updated: 2026-06-03

Uploads extracted Jane Street documentation to Firestore.
"""

import os
import sys
import json
import time
import argparse
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional

import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore

# Configuration
REPOS_DIR = Path.home() / ".jane-street"
COLLECTION_NAME = "jane-street-repo-intel"
CREDENTIALS_PATH = "firebase-credentials.json"
MAX_WRITES_PER_SECOND = 500  # Firestore rate limit
BATCH_SIZE = 100  # Write in batches


def log(message: str, level: str = "INFO"):
    """Log message with timestamp."""
    timestamp = datetime.now().strftime("%H:%M:%S")
    print(f"[{timestamp}] [{level}] {message}")


def init_firestore():
    """Initialize Firebase using local service account credentials."""
    # Resolve absolute path based on the script's location
    root_dir = Path(__file__).parent.parent
    cred_path = root_dir / CREDENTIALS_PATH
    
    if not cred_path.exists():
        log(f"Credentials not found at {cred_path}", "ERROR")
        log("Please ensure firebase-credentials.json is in the project root", "ERROR")
        sys.exit(1)
    
    try:
        cred = credentials.Certificate(str(cred_path))
        if not firebase_admin._apps:
            firebase_admin.initialize_app(cred)
        
        db = firestore.client()
        log("Firestore initialized successfully", "INFO")
        return db
    
    except Exception as e:
        log(f"Failed to initialize Firestore: {e}", "ERROR")
        sys.exit(1)


def load_extracted_docs(repo_name: str) -> Optional[Dict]:
    """Load extracted docs JSON for a repository."""
    docs_file = REPOS_DIR / repo_name / "extracted-docs.json"
    
    if not docs_file.exists():
        log(f"Extracted docs not found: {docs_file}", "WARN")
        return None
    
    try:
        with open(docs_file, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        log(f"Failed to load {docs_file}: {e}", "ERROR")
        return None


def prepare_firestore_document(docs: Dict) -> Dict:
    """Prepare document for Firestore upload (handle size limits)."""
    # Firestore document size limit: 1 MB
    # Truncate large fields if necessary
    
    doc = {
        "repo": docs["repo"],
        "indexed_at": datetime.now().isoformat(),
        "extracted_at": docs.get("extracted_at"),
        "metadata": docs.get("metadata", {}),
    }
    
    # README (truncate to 50KB if needed)
    readme = docs.get("readme")
    if readme:
        if len(readme) > 50000:
            doc["readme"] = readme[:50000] + "\n\n[TRUNCATED]"
            doc["readme_truncated"] = True
        else:
            doc["readme"] = readme
            doc["readme_truncated"] = False
    else:
        doc["readme"] = None
        doc["readme_truncated"] = False
    
    # DESIGN doc (truncate to 50KB if needed)
    design = docs.get("design")
    if design:
        if len(design) > 50000:
            doc["design"] = design[:50000] + "\n\n[TRUNCATED]"
            doc["design_truncated"] = True
        else:
            doc["design"] = design
            doc["design_truncated"] = False
    else:
        doc["design"] = None
        doc["design_truncated"] = False
    
    # Comments (limit to 100 comments, 500 chars each)
    comments = docs.get("comments", [])
    if comments:
        doc["comments"] = [
            {
                "file": c.get("file", "unknown"),
                "comment": c.get("comment", "")[:500]
            }
            for c in comments[:100]
        ]
        doc["comments_count"] = len(comments)
    else:
        doc["comments"] = []
        doc["comments_count"] = 0
    
    # Commits (limit to 100 commits)
    commits = docs.get("commits", [])
    if commits:
        doc["commits"] = commits[:100]
        doc["commits_count"] = len(commits)
    else:
        doc["commits"] = []
        doc["commits_count"] = 0
    
    return doc


def upload_repo_to_firestore(db, repo_name: str, max_retries: int = 3, dry_run: bool = False) -> bool:
    """Upload a single repository's docs to Firestore with retry logic."""
    log(f"{'[DRY-RUN] ' if dry_run else ''}Uploading {repo_name}...", "INFO")
    
    # Load extracted docs
    docs = load_extracted_docs(repo_name)
    if not docs:
        return False
    
    # Prepare Firestore document
    firestore_doc = prepare_firestore_document(docs)
    
    # Validate document size
    doc_size = len(json.dumps(firestore_doc))
    log(f"  Document size: {doc_size:,} bytes ({doc_size / 1024:.1f} KB)", "DEBUG")
    
    if doc_size > 1_000_000:  # 1 MB Firestore limit
        log(f"  WARNING: Document exceeds 1 MB limit ({doc_size / 1024 / 1024:.2f} MB)", "WARN")
        return False
    
    # Dry-run mode: validate only, don't upload
    if dry_run:
        log(f"  [DRY-RUN] Validation passed - would upload {doc_size:,} bytes", "INFO")
        return True
    
    # Upload with retry logic
    for attempt in range(1, max_retries + 1):
        try:
            doc_ref = db.collection(COLLECTION_NAME).document(repo_name)
            doc_ref.set(firestore_doc)
            
            log(f"  Uploaded successfully (attempt {attempt})", "INFO")
            return True
        
        except Exception as e:
            if attempt < max_retries:
                log(f"  Upload failed (attempt {attempt}): {e}", "WARN")
                log(f"  Retrying in {attempt * 2} seconds...", "WARN")
                time.sleep(attempt * 2)
            else:
                log(f"  Upload failed after {max_retries} attempts: {e}", "ERROR")
                return False
    
    return False


def verify_upload(db, repo_name: str) -> bool:
    """Verify that a document was uploaded successfully."""
    try:
        doc_ref = db.collection(COLLECTION_NAME).document(repo_name)
        doc = doc_ref.get()
        
        if doc.exists:
            data = doc.to_dict()
            log(f"  Verified: {repo_name} ({data.get('metadata', {}).get('total_files', 0)} files)", "DEBUG")
            return True
        else:
            log(f"  Verification failed: {repo_name} not found", "ERROR")
            return False
    
    except Exception as e:
        log(f"  Verification error: {e}", "ERROR")
        return False


def main():
    """Main upload workflow."""
    # Parse arguments
    parser = argparse.ArgumentParser(description="Upload Jane Street intel to Firestore")
    parser.add_argument("--dry-run", action="store_true", help="Validate docs without uploading")
    args = parser.parse_args()
    
    log("=== Jane Street Intel Firestore Upload ===", "INFO")
    if args.dry_run:
        log("MODE: DRY-RUN (validation only, no uploads)", "WARN")
    log(f"Repos directory: {REPOS_DIR}", "INFO")
    log(f"Collection: {COLLECTION_NAME}", "INFO")
    
    # Initialize Firestore (skip in dry-run mode)
    if args.dry_run:
        db = None
        log("Skipping Firestore initialization (dry-run mode)", "INFO")
    else:
        db = init_firestore()
    
    # Find all extracted-docs.json files
    extracted_files = list(REPOS_DIR.glob("*/extracted-docs.json"))
    
    if not extracted_files:
        log("No extracted docs found. Run extraction first.", "ERROR")
        return 1
    
    log(f"Found {len(extracted_files)} repos to upload", "INFO")
    
    # Upload each repo
    uploaded_count = 0
    failed_count = 0
    
    for docs_file in extracted_files:
        repo_name = docs_file.parent.name
        
        # Rate limiting (max 500 writes/sec) - skip in dry-run
        if not args.dry_run:
            time.sleep(1.0 / MAX_WRITES_PER_SECOND)
        
        success = upload_repo_to_firestore(db, repo_name, dry_run=args.dry_run)
        
        if success:
            # Verify upload (skip in dry-run)
            if args.dry_run or verify_upload(db, repo_name):
                uploaded_count += 1
            else:
                failed_count += 1
        else:
            failed_count += 1
    
    # Summary
    log("", "INFO")
    log("=== Upload Summary ===", "INFO")
    log(f"Total repos: {len(extracted_files)}", "INFO")
    log(f"Uploaded: {uploaded_count}", "INFO")
    log(f"Failed: {failed_count}", "INFO")
    
    if failed_count > 0:
        log("Some uploads failed. Check logs above.", "WARN")
        return 1
    
    if args.dry_run:
        log("Dry-run complete! All docs validated successfully.", "INFO")
        log("Run without --dry-run to perform actual upload.", "INFO")
    else:
        log("Upload complete!", "INFO")
        log(f"Query with: python scripts/query_kb.py \"<term>\"", "INFO")
    return 0


if __name__ == "__main__":
    exit(main())

# Made with Bob
