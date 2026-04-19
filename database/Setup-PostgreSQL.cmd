@echo off
setlocal
set SCRIPT_DIR=%~dp0
powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Setup-PostgreSQL.ps1" -ProjectRoot "%SCRIPT_DIR%.." %*
if errorlevel 1 (
  echo.
  echo Setup failed.
  pause
  exit /b 1
)
echo.
echo PostgreSQL is ready.
pause
