@echo off
set TOOL_DIR=tool

del /q /s /f out\*.cs
%TOOL_DIR%\thrift-0.9.3.exe -out out  --gen csharp src\WidgetConfig.thrift
copy /Y out\*.cs   ..\..\UCommon\UCore\UI

rem copy /Y out\*.cs   ..\UEditor\UEditor\UI\UIConfig
rem copy /Y out\*.cs   ..\UGame\UGame\UI\UISupport
 
echo success
pause