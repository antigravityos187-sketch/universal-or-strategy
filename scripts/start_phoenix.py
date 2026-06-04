#!/usr/bin/env python3
"""
Start Phoenix real-time trace visualization server.
Access at: http://localhost:6006
"""

import phoenix as px
import sys

def main():
    print("[*] Starting Phoenix trace visualization...")
    print("[*] Dashboard will be available at: http://localhost:6006")
    
    try:
        # Launch Phoenix server
        session = px.launch_app()
        print(f"[+] Phoenix started successfully!")
        print(f"[+] Session URL: {session.url}")
        print("[*] Press Ctrl+C to stop")
        
        # Keep server running
        session.wait()
        
    except KeyboardInterrupt:
        print("\n[*] Shutting down Phoenix...")
        sys.exit(0)
    except Exception as e:
        print(f"[-] Error starting Phoenix: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()

# Made with Bob
