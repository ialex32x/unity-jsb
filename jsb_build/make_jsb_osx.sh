#!/usr/bin/env sh

rm -rf out/osx
mkdir -p out/osx
cd out/osx

cmake -GXcode ../../jsb
cd ..
cmake --build osx --config Release
# cd ..
# mkdir -p ./unity/Assets/Duktape/Plugins/duktape.bundle/Contents/MacOS/
# cp ./build/osx/Release/duktape.bundle/Contents/MacOS/duktape ./unity/Assets/Duktape/Plugins/duktape.bundle/Contents/MacOS/

