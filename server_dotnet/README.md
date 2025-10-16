# Inzynierka.Server (ASP.NET Core)

Minimal ASP.NET Core server that serves the static frontend (../public) and exposes two API endpoints backed by MongoDB:

- GET /api/agents?limit=100
- GET /api/agents/{agentId}

Environment variables:
- MONGODB_URI (default: mongodb://127.0.0.1:27017)
- DB_NAME (default: agentsInzynierka)

Getting started (Windows / PowerShell):

cd server_dotnet
dotnet restore
dotnet run

Then open http://localhost:5000 in your browser (or the port printed by dotnet run).
