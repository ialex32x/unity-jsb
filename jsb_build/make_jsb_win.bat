@echo off

mkdir out
pushd out

rd /s /q win_x64
mkdir win_x64
pushd win_x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\jsb
REM cmake -G "Visual Studio 16 2019" -A x64 ..\..\..
popd
cmake --build x64 --config Release
REM xcopy /Y .\x64\Release\jsb.dll ..\..\unity\Assets\Duktape\Plugins\x64\
popd

popd
