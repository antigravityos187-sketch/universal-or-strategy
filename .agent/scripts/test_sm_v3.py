import requests
import json

def test_v3_documents():
    url = "https://supermemory.ai/api/v3/documents"
    headers = {
        "Authorization": "Bearer sm_VH1nM49Eu7Q6XBe7J1BVrf_jmYgYsthxELbFXbCgZfIzWVlGTHSpmuziDxtHFlFClfSPUYSRqnDnxloBinBjjcY",
        "Content-Type": "application/json"
    }
    
    payload = {
        "content": "TEST SEED: Antigravity Session Verification (Feb 8th)",
        "metadata": {
            "source": "antigravity-ide",
            "model": "opus-4.6"
        }
    }
    
    try:
        print("Testing Supermemory v3 Documents API...")
        response = requests.post(url, headers=headers, json=payload, timeout=30)
        print(f"Status: {response.status_code}")
        print(f"Response: {response.text}")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_v3_documents()
