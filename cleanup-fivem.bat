@echo off
setlocal EnableExtensions EnableDelayedExpansion
color 0f
title FiveM Cache Cleaner
cls

rem ============================================================
rem  FiveM Cache Cleaner - Batch Script
rem  Author: rewindtm
rem  Year: 2025
rem  Description:
rem    This script cleans FiveM cache, logs, crashes,
rem    and temporary system folders to improve stability
rem    and performance. At the end shows total files removed
rem    and space freed.
rem ============================================================

rem === VARIABLES FOR STATS ===
set /a totalFiles=0
set /a totalBytes=0

rem === CONFIGURATION ===
set "FIVEM_DIR=PERCORSO\FiveM.app"

rem === CLEAN FIVEM DIRECTORIES ===
echo [*] Cleaning FiveM in: "%FIVEM_DIR%"

call :ClearDir "%FIVEM_DIR%\logs"
call :ClearDir "%FIVEM_DIR%\crashes"
call :ClearDir "%FIVEM_DIR%\data\cache"
call :ClearDir "%FIVEM_DIR%\data\server-cache"
call :ClearDir "%FIVEM_DIR%\data\server-cache-priv"

echo.
echo [*] Cleaning Windows system folders (requires admin privileges)...

call :ClearDir "C:\Windows\Prefetch"
call :ClearDir "C:\Windows\Temp"
call :ClearDir "%USERPROFILE%\AppData\Local\Temp"

rem === SHOW STATS ===
echo.
echo ============================================================
echo [*] Cleaning complete!
echo [*] Files deleted : %totalFiles%

rem --- Calcolo spazio liberato con PowerShell ---
for /f "usebackq tokens=*" %%A in (`powershell -NoProfile -Command "[Math]::Round(%totalBytes%/1MB,2)"`) do set freedMB=%%A
for /f "usebackq tokens=*" %%A in (`powershell -NoProfile -Command "[Math]::Round(%totalBytes%/1GB,2)"`) do set freedGB=%%A

if %freedGB% gtr 0 (
    echo [*] Space freed  : %freedGB% GB  (^~%freedMB% MB^)
) else (
    echo [*] Space freed  : %freedMB% MB
)
echo ============================================================

echo.
echo [*] Script finished. Press any key to exit...
pause >nul
exit /b

rem ============================================================
rem FUNCTION: ClearDir
rem ------------------------------------------------------------
rem %~1 = directory path to clean
rem Counts files and bytes, then deletes them.
rem ============================================================
:ClearDir
if not exist "%~1" (
  echo [!] Path not found: "%~1"
  exit /b 0
)

set /a files=0
set /a bytes=0

for /r "%~1" %%F in (*) do (
    set /a files+=1
    set /a bytes+=%%~zF
)

set /a totalFiles+=files
set /a totalBytes+=bytes

rem Delete files and subfolders
del /F /Q /S "%~1\*.*" >nul 2>&1
for /D %%D in ("%~1\*") do rd /S /Q "%%~fD" >nul 2>&1

echo [+] Cleaned: "%~1" (files: !files!, size: !bytes! bytes)
exit /b 0
