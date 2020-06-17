@echo off

SET BASE_PATH=%~dp0
SET BUILD_PATH=%~dp0..\out\libwebsockets
echo %BUILD_PATH%
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q x64_release 2>nul
echo building x64_release
mkdir x64_release
cd x64_release
cmake -G "Visual Studio 16 2019" -A x64 ..\..\..\websockets
cd %BUILD_PATH%
cmake --build x64_release --config Release
mkdir ..\..\prebuilt\Plugins\x64\
xcopy /Y .\x64_release\Release\libwebsockets.dll ..\..\prebuilt\Plugins\x64\

REM rd /s /q x86_release 2>nul
REM echo building x86_release
REM mkdir x86_release
REM cd x86_release
REM cmake -G "Visual Studio 16 2019" -A Win32 ..\..\..\websockets
REM cd %BUILD_PATH%
REM cmake --build x86_release --config Release
REM mkdir ..\..\prebuilt\Plugins\x86\
REM xcopy /Y .\x86_release\Release\libwebsockets.dll ..\..\prebuilt\Plugins\x86\

cd %BASE_PATH%
