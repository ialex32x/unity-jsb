@echo off

mkdir out
pushd out

mkdir jsb
pushd jsb
rd /s /q x64
mkdir x64
pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\jsb
REM cmake -G "Visual Studio 16 2019" -A x64 ..\..\..
popd
cmake --build x64 --config Release
REM xcopy /Y .\x64\Release\jsb.dll ..\..\unity\Assets\Duktape\Plugins\x64\
popd

popd
