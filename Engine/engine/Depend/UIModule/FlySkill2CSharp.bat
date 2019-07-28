@echo off
set TOOL_DIR=tool

del /q /s /f out\*.cs
%TOOL_DIR%\thrift-0.9.3.exe -out out  --gen csharp src\FlySkillConfig.thrift
%TOOL_DIR%\thrift-0.9.3.exe -out out  --gen java src\FlySkillConfig.thrift
copy /Y out\*.cs   ..\..\UGame\UGame\Logic\Skill

echo success
pause