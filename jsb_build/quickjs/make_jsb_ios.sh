#!/usr/bin/env sh

rm -rf ../out/quickjs/ios_release
mkdir -p ../out/quickjs/ios_release
cd ../out/quickjs/ios_release
cmake -DQJS_VERSION=2020-07-05 -DCONFIG_LTO=y -DCMAKE_TOOLCHAIN_FILE=../../../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../../../quickjs
cd ..
echo building...
cmake --build ios_release --config Release
mkdir -p ../../prebuilt/Plugins/iOS/
pwd
cp ./ios_release/Release-iphoneos/libquickjs.a ../../prebuilt/Plugins/iOS/
cd ..
