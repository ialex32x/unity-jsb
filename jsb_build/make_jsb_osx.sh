#!/usr/bin/env sh

python jsb_helpers.py version ./quickjs-2019-10-27/VERSION ./jsb/jsb_version.h

rm -rf out/osx
mkdir -p out/osx
cd out/osx

cmake -GXcode ../../jsb
cd ..
cmake --build osx --config Release
cd ..
mkdir -p ../Assets/jsb/Plugins/jsb.bundle/Contents/MacOS/
cp ./out/osx/Release/jsb.bundle/Contents/MacOS/jsb ../Assets/jsb/Plugins/jsb.bundle/Contents/MacOS/
