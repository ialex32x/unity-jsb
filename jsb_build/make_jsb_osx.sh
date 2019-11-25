#!/usr/bin/env sh

rm -rf out/osx
mkdir -p out/osx
cd out/osx

cmake -GXcode ../../jsb
cd ..
cmake --build osx --config Release
cd ..
mkdir -p ../Assets/jsb/Plugins/jsb.bundle/Contents/MacOS/
cp ./build/osx/Release/jsb.bundle/Contents/MacOS/jsb ../Assets/jsb/Plugins/jsb.bundle/Contents/MacOS/
