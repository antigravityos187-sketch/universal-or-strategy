const https = require('https');

// --- CONFIGURATION ---
const REMOTE_URL = 'https://mcp.supermemory.ai/mcp';
const HEADERS = {
    'Authorization': 'Bearer sm_VH1nM49Eu7Q6XBe7J1BVrf_jmYgYsthxELbFXbCgZfIzWVlGTHSpmuziDxtHFlFClfSPUYSRqnDnxloBinBjjcY',
    'x-sm-project': 'universal-or-v12',
    'Content-Type': 'application/json',
    'Accept': 'text/event-stream'
};

const content = process.argv[2] || "Session Anchor: V12.12 Strategic Refactor Initiated";

console.log("🚀 Initializing Supermemory Anchor...");

// 1. Establish SSE Connection to get the POST endpoint
const req = https.get(REMOTE_URL, { headers: HEADERS }, (res) => {
    if (res.statusCode !== 200) {
        console.error(`❌ SSE connection failed: ${res.statusCode}`);
        process.exit(1);
    }

    res.on('data', (chunk) => {
        const lines = chunk.toString().split('\n');
        for (const line of lines) {
            if (line.startsWith('data: ')) {
                const data = line.replace('data: ', '').trim();

                // If it's a URL (relative or absolute), it's our target for the tool call
                if (data.startsWith('/') || data.startsWith('http')) {
                    const postUrl = data.startsWith('/')
                        ? `${new URL(REMOTE_URL).protocol}//${new URL(REMOTE_URL).host}${data}`
                        : data;

                    console.log(`🔗 Found Endpoint: ${postUrl}`);
                    anchorMemory(postUrl);
                }
            }
        }
    });
});

req.on('error', (err) => {
    console.error(`❌ Error: ${err.message}`);
    process.exit(1);
});

function anchorMemory(postUrl) {
    const urlObj = new URL(postUrl);

    // Construct MCP "save" tool call
    const payload = JSON.stringify({
        jsonrpc: "2.0",
        method: "tools/call",
        params: {
            name: "save",
            arguments: {
                content: content
            }
        },
        id: Date.now()
    });

    console.log("💾 Seeding Memory to Supermemory...");

    const postReq = https.request({
        hostname: urlObj.hostname,
        path: urlObj.pathname + urlObj.search,
        method: 'POST',
        headers: {
            ...HEADERS,
            'Accept': 'application/json',
            'Content-Length': Buffer.byteLength(payload)
        }
    }, (res) => {
        if (res.statusCode === 200 || res.statusCode === 202) {
            console.log("✅ SESSION ANCHORED SUCCESSFULLY!");
            process.exit(0);
        } else {
            console.error(`❌ Failed to anchor: ${res.statusCode}`);
            process.exit(1);
        }
    });

    postReq.on('error', (err) => {
        console.error(`❌ POST Error: ${err.message}`);
        process.exit(1);
    });

    postReq.write(payload);
    postReq.end();
}

// Timeout after 15s
setTimeout(() => {
    console.error("❌ Anchor Timeout: Server did not provide POST endpoint.");
    process.exit(1);
}, 15000);
