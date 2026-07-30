# Build React UI and copy to wwwroot
Write-Host "Building React UI..." -ForegroundColor Cyan

Set-Location "$PSScriptRoot\UI\project"

# Install dependencies
Write-Host "Installing dependencies..." -ForegroundColor Yellow
npm install

# Build
Write-Host "Building production bundle..." -ForegroundColor Yellow
npm run build

# Copy to wwwroot root
$wwwrootPath = "$PSScriptRoot\MediTrack.Mvc\wwwroot"
$spaPath = "$PSScriptRoot\MediTrack.Mvc\wwwroot\spa"

# Copy assets
if (Test-Path "$wwwrootPath\assets") { Remove-Item -Path "$wwwrootPath\assets" -Recurse -Force }
Copy-Item -Path "dist\assets" -Destination "$wwwrootPath\assets" -Recurse -Force

# Copy index.html and favicon
Copy-Item -Path "dist\index.html" -Destination "$wwwrootPath\index.html" -Force
Copy-Item -Path "dist\favicon.svg" -Destination "$wwwrootPath\favicon.svg" -Force

Write-Host "Build complete! SPA files copied to wwwroot/" -ForegroundColor Green
Write-Host "Run 'dotnet run' in MediTrack.Mvc to start the app" -ForegroundColor Cyan
