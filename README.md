# Inzynierka — Metrics Dashboard (prototype)

This small prototype contains:
- an Express server that exposes `/api/agents` and serves a static frontend
- a minimal frontend in `public/` that fetches `/api/agents` and displays metric flags

Quick start (PowerShell):

1. Install dependencies:

```powershell
cd C:\Users\Michał\Desktop\wsb\Inzynierka\git_public
npm install
```

2. Start server:

```powershell
npm start
```

3. Open http://localhost:3000 in your browser.

Configuration:
- By default the server connects to `mongodb://127.0.0.1:27017` and uses database `agentsInzynierka`.
- To change, set environment variables `MONGODB_URI` and/or `DB_NAME`.

Notes:
- This is a simple skeleton for local development. For production, add proper error handling, auth, and pagination.
