#!/usr/bin/env sh

python jsb_helpers.py version ./quickjs-2019-10-27/VERSION ./jsb/jsb_version.h

rm -rf out/win_x64
mkdir -p out/win_x64
cd out/win_x64

cmake -G "Unix Makefiles" ../../jsb
cd ..
cmake --build win_x64 --config Release
cd ..
mkdir -p ../Assets/jsb/Plugins/x64/
cp ./out/win_x64/Release/jsb.dll ../Assets/jsb/Plugins/x64/
