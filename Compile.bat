@echo off
dotnet publish -r win-x64 -c Release
dotnet publish -r win-x86 -c Release
pause
