@echo off
setlocal EnableExtensions
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
rem    and performance.
rem ============================================================

rem === CONFIGURATION ===
rem Set the root directory of FiveM installation
set "FIVEM_DIR=C:\GTA 5 e FIVE M\FIVE M\FiveM.app"

rem === CLEAN FIVEM DIRECTORIES ===
echo [*] Cleaning FiveM in: "%FIVEM_DIR%"

rem Call the :ClearDir function for each FiveM folder
call :ClearDir "%FIVEM_DIR%\logs"             rem Logs folder
call :ClearDir "%FIVEM_DIR%\crashes"          rem Crash reports
call :ClearDir "%FIVEM_DIR%\data\cache"       rem Game cache
call :ClearDir "%FIVEM_DIR%\data\server-cache"       rem Server-side cache
call :ClearDir "%FIVEM_DIR%\data\server-cache-priv"  rem Private server cache

echo.
echo [*] Cleaning Windows system folders (requires admin privileges)...

rem === CLEAN WINDOWS DIRECTORIES ===
call :ClearDir "C:\Windows\Prefetch"                  rem Windows Prefetch (launch optimizations)
call :ClearDir "C:\Windows\Temp"                      rem Windows Temp folder
call :ClearDir "%USERPROFILE%\AppData\Local\Temp"     rem User Temp folder

echo.
echo [*] Done.
pause
exit /b

rem ============================================================
rem FUNCTION: ClearDir
rem ------------------------------------------------------------
rem %~1 = directory path to clean
rem Deletes all files and subfolders inside the given directory.
rem ============================================================
:ClearDir
if not exist "%~1" (
  rem If the folder doesn’t exist, display a warning and exit function
  echo [!] Path not found: "%~1"
  exit /b 0
)

rem === DELETE FILES ===
rem /F = force deletion
rem /Q = quiet mode (no confirmation)
rem /S = delete from all subfolders
del /F /Q /S "%~1\*.*" >nul 2>&1

rem === DELETE SUBFOLDERS ===
rem Loop through each directory inside the target folder and remove it
for /D %%D in ("%~1\*") do (
  rd /S /Q "%%~fD" >nul 2>&1
)

rem Print success message
echo [+] Cleaned: "%~1"
exit /b 0
