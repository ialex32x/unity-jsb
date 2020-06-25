#!/usr/bin/env sh

rm -rf ../out/quickjs/macos_release
mkdir -p ../out/quickjs/macos_release
cd ../out/quickjs/macos_release
cmake -DCONFIG_LTO=y -GXcode ../../../quickjs
cd ..
echo building...
cmake --build macos_release --config Release
mkdir -p ../../prebuilt/Plugins/libquickjs.bundle/Contents/MacOS/
pwd
cp ./macos_release/Release/libquickjs.bundle/Contents/MacOS/libquickjs ../../prebuilt/Plugins/libquickjs.bundle/Contents/MacOS
cd ..
