REM @echo off

REM python jsb_helpers.py version .\quickjs-2019-10-27\VERSION .\jsb\jsb_version.h

REM mkdir out
REM pushd out

REM rd /s /q win_x64
REM mkdir win_x64
REM pushd win_x64
REM REM cmake -G "Visual Studio 15 2017 Win64" ..\..\jsb
REM cmake -G "Visual Studio 16 2019" -T ClangCL -A x64 ..\..\jsb
REM popd
REM cmake --build win_x64 --config Release
REM popd

REM xcopy /Y .\out\win_x64\Release\jsb.dll ..\Assets\jsb\Plugins\x64\
