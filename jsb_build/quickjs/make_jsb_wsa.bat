@echo off

@REM set VS_VERSION="Visual Studio 15 2017"
set VS_VERSION="Visual Studio 16 2019"

set BASE_PATH=%~dp0
set BUILD_PATH=%~dp0..\out\quickjs
echo %BUILD_PATH%
echo %VS_VERSION%

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q %BUILD_PATH%\wsa_x86_release 2>nul
echo building wsa_x86_release
mkdir %BUILD_PATH%\wsa_x86_release
cd %BUILD_PATH%\wsa_x86_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WSA=y -DCMAKE_BUILD_TYPE=RELEASE -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 -G %VS_VERSION% -A Win32 %BASE_PATH%
cd %BUILD_PATH%
cmake --build wsa_x86_release --config Release
mkdir ..\..\prebuilt\Plugins\WSA\x86\
xcopy /Y .\wsa_x86_release\Release\quickjs.dll ..\..\prebuilt\Plugins\WSA\x86\

rd /s /q %BUILD_PATH%\wsa_x64_release 2>nul
echo building wsa_x64_release
mkdir %BUILD_PATH%\wsa_x64_release
cd %BUILD_PATH%\wsa_x64_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WSA=y -DCMAKE_BUILD_TYPE=RELEASE -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 -G %VS_VERSION% -A x64 %BASE_PATH%
cd %BUILD_PATH%
cmake --build wsa_x64_release --config Release
mkdir ..\..\prebuilt\Plugins\WSA\x64\
xcopy /Y .\wsa_x64_release\Release\quickjs.dll ..\..\prebuilt\Plugins\WSA\x64\

rd /s /q %BUILD_PATH%\wsa_ARM_release 2>nul
echo building wsa_ARM_release
mkdir %BUILD_PATH%\wsa_ARM_release
cd %BUILD_PATH%\wsa_ARM_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WSA=y -DCMAKE_BUILD_TYPE=RELEASE -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 -G %VS_VERSION% -A ARM %BASE_PATH%
cd %BUILD_PATH%
cmake --build wsa_ARM_release --config Release
mkdir ..\..\prebuilt\Plugins\WSA\ARM\
xcopy /Y .\wsa_ARM_release\Release\quickjs.dll ..\..\prebuilt\Plugins\WSA\ARM\

rd /s /q %BUILD_PATH%\wsa_ARM64_release 2>nul
echo building wsa_ARM64_release
mkdir %BUILD_PATH%\wsa_ARM64_release
cd %BUILD_PATH%\wsa_ARM64_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WSA=y -DCMAKE_BUILD_TYPE=RELEASE -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 -G %VS_VERSION% -A ARM64 %BASE_PATH%
cd %BUILD_PATH%
cmake --build wsa_ARM64_release --config Release
mkdir ..\..\prebuilt\Plugins\WSA\ARM64\
xcopy /Y .\wsa_ARM64_release\Release\quickjs.dll ..\..\prebuilt\Plugins\WSA\ARM64\
