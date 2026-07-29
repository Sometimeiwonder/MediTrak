# Build React UI and copy to wwwroot/spa
Write-Host "Building React UI..." -ForegroundColor Cyan

Set-Location "$PSScriptRoot\UI\project"

# Install dependencies
Write-Host "Installing dependencies..." -ForegroundColor Yellow
npm install

# Build
Write-Host "Building production bundle..." -ForegroundColor Yellow
npm run build

# Copy to wwwroot/spa
$spaPath = "$PSScriptRoot\MediTrack.Mvc\wwwroot\spa"
if (Test-Path $spaPath) { Remove-Item -Path $spaPath -Recurse -Force }
New-Item -ItemType Directory -Path $spaPath -Force | Out-Null

Copy-Item -Path "dist\*" -Destination $spaPath -Recurse -Force

Write-Host "Build complete! SPA files copied to wwwroot/spa" -ForegroundColor Green
Write-Host "Run 'dotnet run' in MediTrack.Mvc to start the app" -ForegroundColor Cyan
