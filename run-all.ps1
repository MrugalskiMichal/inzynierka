# ============================
# RUN ALL PROJECTS IN 3 TABS
# ============================

Write-Host "Starting Agent..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "dotnet run --project `"Agent\Agent\Agent.csproj`""

Start-Sleep -Seconds 20

Write-Host "Starting ManagerServer..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "dotnet run --project `"ManagerServer\ManagerServer\ManagerServer.csproj`""

Start-Sleep -Seconds 1

Write-Host "Starting Frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "dotnet run --project `"Frontend\Frontend.csproj`""

Start-Sleep -Seconds 1

Write-Host "All services started in separate terminals." -ForegroundColor Magenta
