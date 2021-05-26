#!/usr/bin/env sh

rm -rf ../out/quickjs/macos_release
mkdir -p ../out/quickjs/macos_release
cd ../out/quickjs/macos_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -GXcode ../../../quickjs
cd ..
echo building...
cmake --build macos_release --config Release
mkdir -p ../../prebuilt/Plugins/quickjs.bundle/Contents/MacOS/
pwd
ls -l ./macos_release/Release/quickjs.bundle/Contents/MacOS/
cp ./macos_release/Release/quickjs.bundle/Contents/MacOS/quickjs ../../prebuilt/Plugins/quickjs.bundle/Contents/MacOS/
cd ..
