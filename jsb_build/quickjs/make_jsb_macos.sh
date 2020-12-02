#!/usr/bin/env sh

rm -rf ../out/quickjs/macos_release
mkdir -p ../out/quickjs/macos_release
cd ../out/quickjs/macos_release
cmake -DQJS_VERSION=2020-11-08 -DCONFIG_LTO=y -GXcode ../../../quickjs
cd ..
echo building...
cmake --build macos_release --config Release
mkdir -p ../../prebuilt/Plugins/quickjs.bundle/Contents/MacOS/
pwd
cp ./macos_release/Release/quickjs.bundle/Contents/MacOS/quickjs ../../prebuilt/Plugins/quickjs.bundle/Contents/MacOS/
cd ..
