@echo off
echo Building React UI...

cd /d "%~dp0UI\project"

echo Installing dependencies...
call npm install

echo Building production bundle...
call npm run build

echo Copying to wwwroot/spa...
if exist "%~dp0MediTrack.Mvc\wwwroot\spa" rmdir /s /q "%~dp0MediTrack.Mvc\wwwroot\spa"
mkdir "%~dp0MediTrack.Mvc\wwwroot\spa"
xcopy /E /I /Y "dist\*" "%~dp0MediTrack.Mvc\wwwroot\spa"

echo.
echo Build complete! SPA files copied to wwwroot/spa
echo Run 'dotnet run' in MediTrack.Mvc to start the app
pause
