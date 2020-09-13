@echo off

set VS_VERSION="Visual Studio 15 2017"

set BASE_PATH=%~dp0
set BUILD_PATH=%~dp0..\out\quickjs
echo %BUILD_PATH%
echo %VS_VERSION%

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q %BUILD_PATH%\wsa_x64_release 2>nul
echo building wsa_x64_release
mkdir %BUILD_PATH%\wsa_x64_release
cd %BUILD_PATH%\wsa_x64_release
cmake -DQJS_VERSION=2020-07-05 -DCONFIG_LTO=y -DJSB_DEF_LOG2=y -DCMAKE_BUILD_TYPE=RELEASE -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 -G %VS_VERSION% %BASE_PATH%
cd %BUILD_PATH%
REM cmake --build wsa_x64_release --config Release
REM mkdir ..\..\prebuilt\Plugins\WSA\x64\
REM xcopy /Y .\wsa_x64_release\Releasa\quickjs.dll ..\..\prebuilt\Plugins\WSA\x64\
